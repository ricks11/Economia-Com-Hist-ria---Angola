namespace ECHA.Web.Controllers

open Microsoft.AspNetCore.Mvc
open Microsoft.AspNetCore.Authorization
open System.Threading.Tasks
open EconomiaComHistoria.Core.DTOs
open ECHA.Web.Services

[<Authorize(Roles = "SuperAdmin")>]
type AdminController(apiClient: ApiClient) =
    inherit Controller()

    member private this.GetToken() =
        let claim = this.User.FindFirst("AccessToken")
        if isNull claim then null else claim.Value

    [<HttpGet>]
    member this.Permissoes() =
        task {
            let token = this.GetToken()
            match token with
            | null -> return this.RedirectToAction("Login", "Auth") :> IActionResult
            | t ->
                try
                    let! utilizadores = apiClient.ListUtilizadoresAsync(t)
                    return this.View(utilizadores) :> IActionResult
                with
                | :? ApiClientException ->
                    return this.RedirectToAction("Login", "Auth") :> IActionResult
        }

    [<HttpPost>]
    member this.AlterarRole(id: int, novaRole: string) =
        task {
            let token = this.GetToken()
            match token with
            | null -> return this.RedirectToAction("Login", "Auth") :> IActionResult
            | t ->
                try
                    let! success = apiClient.AlterarRoleAsync(id, novaRole, t)
                    if success then
                        this.TempData["SuccessMessage"] <- "Permissão actualizada com sucesso."
                    else
                        this.TempData["ErrorMessage"] <- "Não foi possível alterar a permissão."
                    return this.RedirectToAction("Permissoes") :> IActionResult
                with
                | :? ApiClientException ->
                    return this.RedirectToAction("Login", "Auth") :> IActionResult
        }

    [<HttpGet>]
    member this.Editorial() =
        this.RedirectToAction("Fila", "Moderacao") :> IActionResult
