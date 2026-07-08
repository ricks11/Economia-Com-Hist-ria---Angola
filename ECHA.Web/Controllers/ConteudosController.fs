namespace ECHA.Web.Controllers

open Microsoft.AspNetCore.Mvc
open Microsoft.AspNetCore.Authorization
open System
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

    [<HttpGet>]
    [<AllowAnonymous>]
    member this.Index (tema: string, nivel: string, regiao: string, tipo: string, estado: string, pagina: int) =
        task {
            try
                let p = if pagina = 0 then 1 else pagina
                let! conteudos = apiClient.ListConteudosAsync(?tema = (if String.IsNullOrEmpty tema then None else Some tema),
                                                               ?nivel = (if String.IsNullOrEmpty nivel then None else Some nivel),
                                                               ?regiao = (if String.IsNullOrEmpty regiao then None else Some regiao),
                                                               ?tipo = (if String.IsNullOrEmpty tipo then None else Some tipo),
                                                               ?estado = (if String.IsNullOrEmpty estado then None else Some estado),
                                                               pagina = p)
                return this.View(conteudos) :> IActionResult
            with
            | :? ApiClientException ->
                return this.RedirectToAction("Login", "Auth") :> IActionResult
        }

    [<HttpGet>]
    [<AllowAnonymous>]
    member this.Details (id: int) =
        task {
            try
                let! conteudo = apiClient.GetConteudoAsync(id)
                match conteudo with
                | Some c -> return this.View(c) :> IActionResult
                | None -> return this.NotFound() :> IActionResult
            with
            | :? ApiClientException ->
                return this.RedirectToAction("Login", "Auth") :> IActionResult
        }

    [<HttpGet>]
    member this.Create () =
        this.View()

    [<HttpPost>]
    member this.Create (request: CreateConteudoDto, imagemCapa: IFormFile) =
        task {
            let token = this.GetToken()
            match token with
            | null -> return this.Unauthorized() :> IActionResult
            | t ->
                try
                    let! result = apiClient.CreateConteudoAsync(request, t)
                    match result with
                    | Some c ->
                        if imagemCapa <> null then
                            use stream = imagemCapa.OpenReadStream()
                            let! _ = apiClient.UploadImagemCapaAsync(c.Id, stream, imagemCapa.FileName, t)
                            ()
                        this.TempData["SuccessMessage"] <- "Conteúdo criado com sucesso!"
                        return this.RedirectToAction("Details", {| id = c.Id |}) :> IActionResult
                    | None ->
                        this.ModelState.AddModelError("", "Falha ao criar conteúdo")
                        return this.View(request) :> IActionResult
                with
                | :? ApiClientException ->
                    return this.RedirectToAction("Login", "Auth") :> IActionResult
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
            | :? ApiClientException ->
                return this.RedirectToAction("Login", "Auth") :> IActionResult
        }

    [<HttpPost>]
    member this.Edit (id: int, request: UpdateConteudoDto, imagemCapa: IFormFile) =
        task {
            let token = this.GetToken()
            match token with
            | null -> return this.Unauthorized() :> IActionResult
            | t ->
                try
                    let! result = apiClient.UpdateConteudoAsync(id, request, t)
                    match result with
                    | Some c ->
                        if imagemCapa <> null then
                            use stream = imagemCapa.OpenReadStream()
                            let! _ = apiClient.UploadImagemCapaAsync(c.Id, stream, imagemCapa.FileName, t)
                            ()
                        this.TempData["SuccessMessage"] <- "Conteúdo atualizado com sucesso!"
                        return this.RedirectToAction("Details", {| id = c.Id |}) :> IActionResult
                    | None ->
                        this.ModelState.AddModelError("", "Falha ao atualizar conteúdo")
                        return this.View(request) :> IActionResult
                with
                | :? ApiClientException ->
                    return this.RedirectToAction("Login", "Auth") :> IActionResult
        }

    [<HttpPost>]
    member this.Delete (id: int) =
        task {
            let token = this.GetToken()
            match token with
            | null -> return this.Unauthorized() :> IActionResult
            | t ->
                try
                    let! success = apiClient.DeleteConteudoAsync(id, t)
                    if success then
                        this.TempData["SuccessMessage"] <- "Conteúdo excluído com sucesso!"
                        return this.RedirectToAction("Index") :> IActionResult
                    else
                        return this.BadRequest("Falha ao eliminar conteúdo") :> IActionResult
                with
                | :? ApiClientException ->
                    return this.RedirectToAction("Login", "Auth") :> IActionResult
        }