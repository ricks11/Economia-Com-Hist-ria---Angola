namespace ECHA.Web.Controllers

open Microsoft.AspNetCore.Mvc
open Microsoft.AspNetCore.Authorization
open System.Threading.Tasks
open EconomiaComHistoria.Core.DTOs
open ECHA.Web.Services

[<Authorize>]
type PerfilController(apiClient: ApiClient) =
    inherit Controller()

    member private this.GetToken() =
        let claim = this.User.FindFirst("AccessToken")
        if isNull claim then null else claim.Value

    [<HttpGet>]
    member this.Index(tab: string) =
        task {
            let token = this.GetToken()
            match token with
            | null -> return this.RedirectToAction("Login", "Auth") :> IActionResult
            | t ->
                try
                    let! perfil = apiClient.GetPerfilAsync(t)
                    let! progresso = apiClient.GetProgressoAsync(t)
                    match perfil with
                    | Some p ->
                        this.ViewData.["Progresso"] <- progresso
                        this.ViewData.["Tab"] <- if System.String.IsNullOrEmpty tab then "perfil" else tab
                        return this.View(p) :> IActionResult
                    | None -> return this.NotFound() :> IActionResult
                with
                | :? ApiClientException ->
                    return this.RedirectToAction("Login", "Auth") :> IActionResult
        }

    [<HttpPost>]
    member this.Atualizar(request: UpdatePerfilDto) =
        task {
            let token = this.GetToken()
            match token with
            | null -> return this.RedirectToAction("Login", "Auth") :> IActionResult
            | t ->
                try
                    let! sucesso = apiClient.UpdatePerfilAsync(request, t)
                    if sucesso then
                        this.TempData["SuccessMessage"] <- "Perfil actualizado com sucesso!"
                    else
                        this.TempData["ErrorMessage"] <- "Não foi possível actualizar o perfil."
                    return this.RedirectToAction("Index") :> IActionResult
                with
                | :? ApiClientException ->
                    return this.RedirectToAction("Login", "Auth") :> IActionResult
        }
