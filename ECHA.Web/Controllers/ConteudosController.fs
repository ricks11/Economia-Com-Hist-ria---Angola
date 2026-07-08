namespace ECHA.Web.Controllers

open Microsoft.AspNetCore.Mvc
open Microsoft.AspNetCore.Authorization
open System
open System.Net
open System.Threading.Tasks
open EconomiaComHistoria.Core.DTOs
open Microsoft.AspNetCore.Http
open ECHA.Web.Services

[<Authorize(Roles = "Editor,Admin")>]
type ConteudosController (apiClient: ECHA.Web.Services.ApiClient) =
    inherit Controller()

    member private this.GetToken() =
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
            
                // Converte a string da URL ("true"/"false") num bool opcional
                let filtrarJindungo = 
                    if String.IsNullOrEmpty jindungo then None
                    elif jindungo.Equals("true", StringComparison.OrdinalIgnoreCase) then Some true
                    elif jindungo.Equals("false", StringComparison.OrdinalIgnoreCase) then Some false
                    else None

                let! conteudos = apiClient.ListConteudosAsync(
                                    ?tema = (if String.IsNullOrEmpty tema then None else Some tema),
                                    ?nivel = (if String.IsNullOrEmpty nivel then None else Some nivel),
                                    ?regiao = (if String.IsNullOrEmpty regiao then None else Some regiao),
                                    ?tipo = (if String.IsNullOrEmpty tipo then None else Some tipo),
                                    ?estado = (if String.IsNullOrEmpty estado then None else Some estado),
                                    ?jindungo = filtrarJindungo, // 👈 Passa o filtro mapeado para a API
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
                // Dispara o incremento (não precisa esperar o resultado para exibir o conteúdo)
                let _ = apiClient.IncrementarVisitasAsync(id) 
            
                let! conteudo = apiClient.GetConteudoAsync(id)
                match conteudo with
                | Some c -> return this.View(c) :> IActionResult
                | None -> return this.NotFound() :> IActionResult
            with
            | _ -> return this.RedirectToAction("Login", "Auth") :> IActionResult
        }

    [<HttpGet>]
    member this.Create () = this.View()

    [<HttpPost>]
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
                        // Aqui vai aparecer o JSON da API a dizer qual campo falhou!
                        this.ModelState.AddModelError("", sprintf "Erro de Validação da API: %s" ex.Message)
                        return this.View(request) :> IActionResult
                | ex ->
                    this.ModelState.AddModelError("", sprintf "Erro interno: %s" ex.Message)
                    return this.View(request) :> IActionResult
        }

    [<HttpGet>]
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