namespace ECHA.Web.Controllers

open Microsoft.AspNetCore.Mvc
open Microsoft.AspNetCore.Authorization
open System
open System.Threading.Tasks
open EconomiaComHistoria.Core.DTOs
open EconomiaComHistoria.Core.Enums
open ECHA.Web.Services

[<Route("Forum")>]
type ForumController(apiClient: ApiClient) =
    inherit Controller()

    member private this.GetToken() =
        let claim = this.User.FindFirst("AccessToken")
        if isNull claim then null else claim.Value

    [<HttpGet>]
    [<Route("Index")>]
    [<AllowAnonymous>]
    member this.Index(categoriaId: Nullable<int>, ordem: string, incluirArquivados: Nullable<bool>) =
        task {
            try
                let token = this.GetToken()
                let tokenOpt = if String.IsNullOrEmpty token then None else Some token
                let categoriaIdOpt = if categoriaId.HasValue then Some categoriaId.Value else None
                let ordemOpt = if String.IsNullOrEmpty ordem then None else Some ordem
                let incluirArquivadosOpt = if incluirArquivados.HasValue then Some incluirArquivados.Value else None

                let! topicos = apiClient.ListTopicosAsync(
                    ?categoriaId = categoriaIdOpt,
                    ?ordem = ordemOpt,
                    ?token = tokenOpt,
                    ?incluirArquivados = incluirArquivadosOpt)

                let! categorias = apiClient.ListCategoriasForumAsync()
                this.ViewData.["Categorias"] <- categorias
                this.ViewData.["CategoriaAtiva"] <- categoriaId
                this.ViewData.["OrdemAtiva"] <- ordem
                this.ViewData.["IncluirArquivados"] <- incluirArquivados.GetValueOrDefault(false)
                return this.View(topicos) :> IActionResult
            with
            | :? ApiClientException -> return this.RedirectToAction("Login", "Auth") :> IActionResult
        }

    [<HttpGet>]
    [<Route("Criar")>]
    [<Authorize>]
    member this.Criar() =
        task {
            let! categorias = apiClient.ListCategoriasForumAsync()
            this.ViewData.["Categorias"] <- categorias
            return this.View() :> IActionResult
        }

    [<HttpPost>]
    [<Route("Criar")>]
    [<Authorize>]
    member this.Criar(request: CriarTopicoForumDto) =
        task {
            let token = this.GetToken()
            match token with
            | null -> return this.RedirectToAction("Login", "Auth") :> IActionResult
            | t ->
                try
                    if not this.ModelState.IsValid then
                        let! categorias = apiClient.ListCategoriasForumAsync()
                        this.ViewData.["Categorias"] <- categorias
                        return this.View(request) :> IActionResult
                    else
                        let! topico = apiClient.CriarTopicoAsync(request, t)
                        match topico with
                        | Some created ->
                            this.TempData["SuccessMessage"] <- "Tópico enviado para revisão! A equipa de moderação irá analisar em breve."
                            return this.RedirectToAction("AguardarAprovacao", "Forum", {| id = created.Id |}) :> IActionResult
                        | None ->
                            this.ModelState.AddModelError("", "Não foi possível criar o tópico.")
                            let! categorias = apiClient.ListCategoriasForumAsync()
                            this.ViewData.["Categorias"] <- categorias
                            return this.View(request) :> IActionResult
                with
                | :? ApiClientException -> return this.RedirectToAction("Login", "Auth") :> IActionResult
        }

    [<HttpGet>]
    [<Route("Details/{id}")>]
    [<AllowAnonymous>]
    member this.Details(id: int) =
        task {
            try
                let token = this.GetToken()
                let tokenOpt = if String.IsNullOrEmpty token then None else Some token
                let! topico = apiClient.GetTopicoAsync(id, ?token = tokenOpt)
                match topico with
                | Some t when t.Estado = EstadoTopicoForum.Ativo ->
                    return this.View(t) :> IActionResult
                | Some t when (t.Estado = EstadoTopicoForum.Pendente || t.Estado = EstadoTopicoForum.Arquivado || t.Estado = EstadoTopicoForum.Rejeitado) && this.User.Identity.IsAuthenticated ->
                    return this.View(t) :> IActionResult
                | _ -> return this.NotFound() :> IActionResult
            with
            | :? ApiClientException -> return this.RedirectToAction("Login", "Auth") :> IActionResult
        }

    [<HttpPost>]
    [<Route("Responder")>]
    [<Authorize>]
    member this.Responder(topicoId: int, conteudo: string, respostaPaiId: Nullable<int>) =
        task {
            let token = this.GetToken()
            match token with
            | null -> return this.RedirectToAction("Login", "Auth") :> IActionResult
            | t ->
                try
                    let request = CriarRespostaForumDto(Conteudo = conteudo, RespostaPaiId = respostaPaiId)
                    let! result = apiClient.AdicionarRespostaAsync(topicoId, request, t)
                    if result then
                        this.TempData["SuccessMessage"] <- "Resposta publicada com sucesso!"
                    else
                        this.TempData["ErrorMessage"] <- "Não foi possível publicar a resposta."
                    return this.RedirectToAction("Details", "Forum", {| id = topicoId |}) :> IActionResult
                with
                | :? ApiClientException ->
                    return this.RedirectToAction("Login", "Auth") :> IActionResult
        }

    [<HttpPost>]
    [<Route("ApagarTopico")>]
    [<Authorize>]
    member this.ApagarTopico(id: int) =
        task {
            let token = this.GetToken()
            match token with
            | null -> return this.RedirectToAction("Login", "Auth") :> IActionResult
            | t ->
                try
                    let! sucesso = apiClient.ApagarTopicoAsync(id, t)
                    if sucesso then
                        this.TempData["SuccessMessage"] <- "Tópico arquivado com sucesso."
                    else
                        this.TempData["ErrorMessage"] <- "Não foi possível arquivar o tópico."
                    return this.RedirectToAction("Index", "Forum") :> IActionResult
                with
                | :? ApiClientException -> return this.RedirectToAction("Login", "Auth") :> IActionResult
        }

    [<HttpPost>]
    [<Route("DesarquivarTopico")>]
    [<Authorize>]
    member this.DesarquivarTopico(id: int) =
        task {
            let token = this.GetToken()
            match token with
            | null -> return this.RedirectToAction("Login", "Auth") :> IActionResult
            | t ->
                try
                    let! sucesso = apiClient.DesarquivarTopicoAsync(id, t) // novo método no ApiClient
                    if sucesso then
                        this.TempData["SuccessMessage"] <- "Tópico desarquivado com sucesso."
                    else
                        this.TempData["ErrorMessage"] <- "Não foi possível desarquivar o tópico."
                    return this.RedirectToAction("Details", "Forum", {| id = id |}) :> IActionResult
                with
                | :? ApiClientException -> return this.RedirectToAction("Login", "Auth") :> IActionResult
        }

    [<HttpPost>]
    [<Route("Denunciar")>]
    [<Authorize>]
    member this.Denunciar(topicoId: Nullable<int>, respostaId: Nullable<int>, motivo: MotivoDenuncia, descricao: string) =
        task {
            let token = this.GetToken()
            match token with
            | null -> return this.RedirectToAction("Login", "Auth") :> IActionResult
            | t ->
                try
                    let request = CriarDenunciaDto(topicoId, respostaId, motivo, (if String.IsNullOrWhiteSpace descricao then null else descricao))
                    let! _ = apiClient.DenunciarAsync(request, t)
                    this.TempData["SuccessMessage"] <- "Denúncia enviada. A nossa equipa vai analisar."

                    // Determina o ID do tópico para redirecionamento
                    let! redirectIdTask =
                        task {
                            if topicoId.HasValue then
                                return topicoId.Value
                            elif respostaId.HasValue then
                                let! respostaOpt = apiClient.ObterRespostaAsync(respostaId.Value, ?token = Some t)
                                return match respostaOpt with | Some r -> r.TopicoId | None -> 0
                            else
                                return 0
                        }

                    if redirectIdTask > 0 then
                        return this.RedirectToAction("Details", "Forum", {| id = redirectIdTask |}) :> IActionResult
                    else
                        return this.RedirectToAction("Index", "Forum") :> IActionResult
                with
                | :? ApiClientException -> return this.RedirectToAction("Login", "Auth") :> IActionResult
        }

    [<HttpGet>]
    [<Route("AguardarAprovacao/{id}")>]
    [<Authorize>]
    member this.AguardarAprovacao(id: int) =
        task {
            let token = this.GetToken()
            let tokenOpt = if String.IsNullOrEmpty token then None else Some token
            match token with
            | null -> return this.RedirectToAction("Login", "Auth") :> IActionResult
            | t ->
                try
                    let! topico = apiClient.GetTopicoAsync(id, ?token = tokenOpt)
                    match topico with
                    | Some t when t.Estado = EstadoTopicoForum.Pendente || t.Estado = EstadoTopicoForum.Rejeitado ->
                        return this.View(t) :> IActionResult
                    | Some t when t.Estado = EstadoTopicoForum.Ativo ->
                        return this.RedirectToAction("Details", "Forum", {| id = id |}) :> IActionResult
                    | _ -> return this.NotFound() :> IActionResult
                with
                | :? ApiClientException -> return this.RedirectToAction("Login", "Auth") :> IActionResult
        }

    [<HttpPost>]
    [<Route("Reagir")>]
    [<Authorize>]
    member this.Reagir(topicoId: Nullable<int>, respostaId: Nullable<int>) =
        task {
            let token = this.GetToken()
            match token with
            | null -> return this.RedirectToAction("Login", "Auth") :> IActionResult
            | t ->
                try
                    let request =
                        if respostaId.HasValue then
                            CriarReacaoDto(
                                TopicoForumId = Nullable<int>(),
                                RespostaForumId = respostaId,
                                Emoji = "👍")
                        else
                            CriarReacaoDto(
                                TopicoForumId = topicoId,
                                RespostaForumId = Nullable<int>(),
                                Emoji = "👍")

                    let! _ = apiClient.ToggleReacaoAsync(request, t)
                    let redirectId = if topicoId.HasValue then topicoId.Value else 0
                    return this.RedirectToAction("Details", "Forum", {| id = redirectId |}) :> IActionResult
                with
                | :? ApiClientException -> return this.RedirectToAction("Login", "Auth") :> IActionResult
        }