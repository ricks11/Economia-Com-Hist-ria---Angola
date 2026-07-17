namespace ECHA.Web.Controllers

open Microsoft.AspNetCore.Mvc
open Microsoft.AspNetCore.Authorization
open System
open System.Net
open System.Threading.Tasks
open EconomiaComHistoria.Web.Models
open EconomiaComHistoria.Core.DTOs
open Microsoft.AspNetCore.Http
open ECHA.Web.Services

[<Authorize(AuthenticationSchemes = "CookieAuthentication")>]
type ConteudosController (apiClient: ECHA.Web.Services.ApiClient) =
    inherit Controller()

    member private this.GetToken() =
        let authHeader =
            this.HttpContext.Request.Headers["Authorization"]
            |> Seq.tryHead
        match authHeader with
        | Some header when header.StartsWith("Bearer ") -> header.Substring("Bearer ".Length)
        | _ ->
            // fallback: tenta a claim antiga
            let claim = this.User.FindFirst("AccessToken")
            if isNull claim then null else claim.Value

    // Mapeador local auxiliar de propriedades da View para o Update/Create do Backend API
    member private this.PrepararUpdateDto (form: IFormCollection) =
        let isJindungoChecked = form.["IsJindungo"].ToString() = "true"
        let mediaUrl = form.["UrlMedia"].ToString()
        let tipoFormat = form.["Tipo"].ToString()
        
        let dto = new UpdateConteudoDto()
        dto.Titulo <- form.["Titulo"].ToString()
        dto.Resumo <- form.["Resumo"].ToString()
        dto.CorpoTexto <- form.["Texto"].ToString() // Sincroniza Texto com CorpoTexto da API
        dto.Tema <- form.["Tema"].ToString()
        dto.Regiao <- form.["Regiao"].ToString()
        dto.IsJindungo <- Nullable<bool>(isJindungoChecked)
        dto.ReferenciaFactual <- if isJindungoChecked then form.["ReferenciaFactual"].ToString() else null
        
        // Mapeia de acordo com o tipo escolhido
        if tipoFormat = "Video" then dto.VideoUrl <- mediaUrl else dto.VideoUrl <- null
        if tipoFormat = "Audio" then dto.AudioUrl <- mediaUrl else dto.AudioUrl <- null
        dto

    [<HttpGet>]
    [<AllowAnonymous>]
    member this.Index (tema: string, nivel: string, regiao: string, tipo: string, estado: string, jindungo: string, pagina: int) =
        task {
            try
                let p = if pagina = 0 then 1 else pagina
        
                let filtrarJindungo = 
                    if String.IsNullOrEmpty jindungo then None
                    elif jindungo.Equals("true", StringComparison.OrdinalIgnoreCase) then Some true
                    elif jindungo.Equals("false", StringComparison.OrdinalIgnoreCase) then Some false
                    else None

                // Função auxiliar local para converter string vazia ou nula em None
                let validarFiltro str = 
                    if String.IsNullOrWhiteSpace str then None else Some str

                let! conteudos = apiClient.ListConteudosAsync(
                                    ?tema = validarFiltro tema,
                                    ?nivel = validarFiltro nivel,
                                    ?regiao = validarFiltro regiao,
                                    ?tipo = validarFiltro tipo,
                                    ?estado = validarFiltro estado,
                                    ?jindungo = filtrarJindungo,
                                    pagina = p)
                return this.View(conteudos) :> IActionResult
            with
            | :? ApiClientException -> return this.RedirectToAction("Login", "Auth") :> IActionResult
        }

    [<HttpGet>]
    [<AllowAnonymous>]
    member this.Details (id: int) =
        task {
            try
                // Protege o incremento de visitas
                try 
                    let! _ = apiClient.IncrementarVisitasAsync(id) |> Async.AwaitTask |> Async.StartChild 
                    ()
                with _ -> () 
    
                // 1. Tentar ler o ID do utilizador logado a partir dos Claims
                let userIdClaim = this.User.FindFirst("sub")
                let userIdClaim = 
                    if isNull userIdClaim then 
                        this.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)
                    else 
                        userIdClaim

                let userIdOpt =
                    if not (isNull userIdClaim) && not (System.String.IsNullOrEmpty(userIdClaim.Value)) then
                        match System.Int32.TryParse(userIdClaim.Value) with
                        | true, uid -> Some uid
                        | _ -> None
                    else 
                        None

                // 2. Recuperar o token para enviar ao backend (para validar Roles/Acessos)
                let token = this.GetToken()
                let tokenOpt = if String.IsNullOrEmpty(token) then None else Some token

                // 3. Chamar a API passando o ID, o token opcional e o userId opcional
                let! conteudo = 
                    match userIdOpt with
                    | Some uid -> apiClient.GetConteudoAsync(id, ?token = tokenOpt, userId = uid)
                    | None -> apiClient.GetConteudoAsync(id, ?token = tokenOpt)

                match conteudo with
                | Some c -> 
                    // 4. Prioriza o TempData de clique recente, senão cai no estado real da BD
                    let estadoFavorito =
                        if this.TempData.ContainsKey("IsFavorito_" + string id) then
                            this.TempData.["IsFavorito_" + string id] :?> bool
                        else
                            c.EhFavorito 

                    // 5. Lê o status de solicitação que possa estar no ViewData (ou assume "Nenhuma")
                    let solicitacaoStatus =
                        match this.ViewData.["SolicitacaoStatus"] with
                        | null -> "Nenhuma"
                        | status -> string status

                    // 6. Calcula se o utilizador atual é um Administrador/Editor
                    let isAdmin = 
                        if isNull this.User || isNull this.User.Identity then false
                        else 
                            this.User.Identity.IsAuthenticated && 
                            (this.User.IsInRole("Admin") || 
                             this.User.IsInRole("Editor") || 
                             this.User.IsInRole("SuperAdmin") || 
                             this.User.IsInRole("Professor"))

                    // 7. Monta a ViewModel robusta (qualificando o primeiro campo para ajudar o compilador)
                    let viewModel = {
                        ConteudoDetailsViewModel.Conteudo = c
                        IsFavorito = estadoFavorito
                        SolicitacaoStatus = solicitacaoStatus
                        IsAdmin = isAdmin
                    }

                    return this.View(viewModel) :> IActionResult
                | None -> 
                    return this.NotFound() :> IActionResult
            with
            | _ -> 
                return this.RedirectToAction("Login", "Auth") :> IActionResult
        }

    [<HttpGet>]
    [<Authorize(Roles = "Editor,Admin,SuperAdmin,Professor")>]
    member this.Create () = this.View()

    [<HttpPost>]
    [<Authorize(Roles = "Editor,Admin,SuperAdmin,Professor")>]
    [<ValidateAntiForgeryToken>]
    member this.Create (request: CreateConteudoDto, imagemCapa: IFormFile) =
        task {
            let token = this.GetToken()
            match token with
            | null -> 
                return this.RedirectToAction("Login", "Auth") :> IActionResult
            | t ->
                if String.IsNullOrEmpty(request.CorpoTexto) then
                    let formTexto = this.Request.Form.["CorpoTexto"].ToString()
                    if not (String.IsNullOrEmpty(formTexto)) then 
                        request.CorpoTexto <- formTexto

                try
                    let! result = apiClient.CreateConteudoAsync(request, t)
                    match result with
                    | Some c ->
                        if imagemCapa <> null && imagemCapa.Length > 0L then
                            try
                                use stream = imagemCapa.OpenReadStream()
                                let! _ = apiClient.UploadImagemCapaAsync(c.Id, stream, imagemCapa.FileName, t)
                                ()
                            with _ -> 
                                this.TempData["WarningMessage"] <- "Conteúdo criado, mas a imagem falhou."

                        this.TempData["SuccessMessage"] <- "Conteúdo criado com sucesso!"
                        return this.RedirectToAction("Index") :> IActionResult
                    | None ->
                        this.ModelState.AddModelError("", "Falha desconhecida ao criar conteúdo.")
                        return this.View(request) :> IActionResult
                with
                | :? ApiClientException as ex -> 
                    if ex.StatusCode = System.Net.HttpStatusCode.Unauthorized then
                        return this.RedirectToAction("Login", "Auth") :> IActionResult
                    else
                        this.ModelState.AddModelError("", sprintf "Erro de Validação da API: %s" ex.Message)
                        return this.View(request) :> IActionResult
                | ex ->
                    this.ModelState.AddModelError("", sprintf "Erro interno: %s" ex.Message)
                    return this.View(request) :> IActionResult
        }

    [<HttpGet>]
    [<Authorize(Roles = "Editor,Admin,SuperAdmin,Professor")>]
    member this.Edit (id: int) =
        task {
            try
                let! conteudo = apiClient.GetConteudoAsync(id)
                match conteudo with
                | Some c -> return this.View(c) :> IActionResult
                | None -> return this.NotFound() :> IActionResult
            with
            | :? ApiClientException -> return this.RedirectToAction("Login", "Auth") :> IActionResult
        }

    [<HttpPost>]
    [<Authorize(Roles = "Editor,Admin,SuperAdmin,Professor")>]
    member this.Edit (id: int, imagemCapa: IFormFile) =
        task {
            let token = this.GetToken()
            match token with
            | null -> return this.Unauthorized() :> IActionResult
            | t ->
                try
                    // Constrói o DTO atualizado explicitamente a partir do Form para evitar perda de dados
                    let requestDto = this.PrepararUpdateDto(this.Request.Form)
                    let! result = apiClient.UpdateConteudoAsync(id, requestDto, t)
                    match result with
                    | Some c ->
                        if imagemCapa <> null then
                            use stream = imagemCapa.OpenReadStream()
                            let! _ = apiClient.UploadImagemCapaAsync(c.Id, stream, imagemCapa.FileName, t)
                            ()
                        this.TempData["SuccessMessage"] <- "Conteúdo atualizado com sucesso!"
                        return this.RedirectToAction("Details", {| id = id |}) :> IActionResult
                    | None ->
                        this.ModelState.AddModelError("", "Falha ao atualizar conteúdo na API Backend")
                        return this.View() :> IActionResult
                with
                | :? ApiClientException -> return this.RedirectToAction("Login", "Auth") :> IActionResult
        }   

    [<HttpPost>]
    [<Authorize(Roles = "Editor,Admin,SuperAdmin,Professor")>]
    [<ValidateAntiForgeryToken>]
    member this.Delete (id: int) =
        task {
            let token = this.GetToken()
            match token with
            | null -> return this.RedirectToAction("Login", "Auth") :> IActionResult
            | t ->
                try
                    let! sucesso = apiClient.DeleteConteudoAsync(id, t)
                    if sucesso then
                        this.TempData["SuccessMessage"] <- "Conteúdo arquivado com sucesso!"
                    else
                        this.TempData["ErrorMessage"] <- "Não foi possível eliminar o conteúdo."
                    return this.RedirectToAction("Index") :> IActionResult
                with
                | :? ApiClientException -> return this.RedirectToAction("Login", "Auth") :> IActionResult
        }

    [<HttpPost>]
    [<ValidateAntiForgeryToken>]
    member this.ToggleFavorito (id: int) =
        task {
            let token = this.GetToken()
            match token with
            | null -> return this.RedirectToAction("Login", "Auth") :> IActionResult
            | t ->
                try
                    // Executa a chamada à API Backend
                    let! estadoFinalFavorito = apiClient.ToggleFavoritoAsync(id, t)
                    
                    // Guarda o estado final para a View ler imediatamente após o redirecionamento
                    this.TempData.["IsFavorito_" + string id] <- estadoFinalFavorito
                    
                    if estadoFinalFavorito then
                        this.TempData["SuccessMessage"] <- "Adicionado aos teus favoritos com sucesso."
                    else
                        this.TempData["SuccessMessage"] <- "Removido dos teus favoritos."
                    
                    return this.RedirectToAction("Details", {| id = id |}) :> IActionResult
                with
                | :? ApiClientException -> return this.RedirectToAction("Login", "Auth") :> IActionResult
                | _ -> 
                    this.TempData["ErrorMessage"] <- "Erro ao processar o favorito."
                    return this.RedirectToAction("Details", {| id = id |}) :> IActionResult
        }


