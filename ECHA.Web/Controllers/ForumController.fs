namespace ECHA.Web.Controllers

open Microsoft.AspNetCore.Mvc
open Microsoft.AspNetCore.Authorization
open System
open System.Threading.Tasks
open EconomiaComHistoria.Core.DTOs
open ECHA.Web.Services

type ForumController(apiClient: ApiClient) =
    inherit Controller()

    member private this.GetToken() =
        let claim = this.User.FindFirst("AccessToken")
        if isNull claim then null else claim.Value

    [<HttpGet>]
    [<AllowAnonymous>]
    member this.Index(categoriaId: Nullable<int>, ordem: string) =
        task {
            try
                let categoriaIdOpt = if categoriaId.HasValue then Some categoriaId.Value else None
                let ordemOpt = if String.IsNullOrEmpty ordem then None else Some ordem
                let! topicos = apiClient.ListTopicosAsync(?categoriaId = categoriaIdOpt, ?ordem = ordemOpt)
                return this.View(topicos) :> IActionResult
            with
            | :? ApiClientException ->
                return this.RedirectToAction("Login", "Auth") :> IActionResult
        }

    [<HttpGet>]
    [<Authorize>]
    member this.Criar() =
        task {
            let! categorias = apiClient.ListCategoriasForumAsync()
            this.ViewData.["Categorias"] <- categorias
            return this.View() :> IActionResult
        }

    [<HttpPost>]
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
                            this.TempData["SuccessMessage"] <- "Tópico criado com sucesso! Pode estar sujeito a moderação."
                            return this.RedirectToAction("Details", {| id = created.Id |}) :> IActionResult
                        | None ->
                            this.ModelState.AddModelError("", "Não foi possível criar o tópico.")
                            let! categorias = apiClient.ListCategoriasForumAsync()
                            this.ViewData.["Categorias"] <- categorias
                            return this.View(request) :> IActionResult
                with
                | :? ApiClientException ->
                    return this.RedirectToAction("Login", "Auth") :> IActionResult
        }

    [<HttpGet>]
    [<AllowAnonymous>]
    member this.Details(id: int) =
        task {
            try
                let! topico = apiClient.GetTopicoAsync(id)
                match topico with
                | Some t -> return this.View(t) :> IActionResult
                | None -> return this.NotFound() :> IActionResult
            with
            | :? ApiClientException ->
                return this.RedirectToAction("Login", "Auth") :> IActionResult
        }

    [<HttpPost>]
    [<Authorize>]
    member this.Responder(topicoId: int, conteudo: string, respostaPaiId: int option) =
        task {
            let token = this.GetToken()
            match token with
            | null -> return this.RedirectToAction("Login", "Auth") :> IActionResult
            | t ->
                try
                    let request = CriarRespostaForumDto(Conteudo = conteudo, RespostaPaiId = Option.toNullable respostaPaiId)
                    let! success = apiClient.AdicionarRespostaAsync(topicoId, request, t)
                    if success then
                        this.TempData["SuccessMessage"] <- "Resposta publicada com sucesso!"
                    else
                        this.TempData["ErrorMessage"] <- "Não foi possível publicar a resposta."
                    return this.RedirectToAction("Details", {| id = topicoId |}) :> IActionResult
                with
                | :? ApiClientException ->
                    return this.RedirectToAction("Login", "Auth") :> IActionResult
        }