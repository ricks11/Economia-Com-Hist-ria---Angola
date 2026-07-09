namespace ECHA.Web.Controllers

open Microsoft.AspNetCore.Mvc
open Microsoft.AspNetCore.Authorization
open System
open System.Threading.Tasks
open EconomiaComHistoria.Core.DTOs
open ECHA.Web.Services

[<Authorize(Roles = "SuperAdmin")>]
type AdminController (apiClient: ApiClient) =
    inherit Controller()

    member private this.GetToken() =
        let claim = this.User.FindFirst("AccessToken")
        if isNull claim then null else claim.Value

    [<HttpGet>]
    member this.Auditoria (utilizadorId: Nullable<int>, acao: string, inicio: Nullable<DateTime>, fim: Nullable<DateTime>) =
        task {
            let token = this.GetToken()
            match token with
            | null -> return this.RedirectToAction("Login", "Auth") :> IActionResult
            | t ->
                try
                    let uId = if utilizadorId.HasValue then Some utilizadorId.Value else None
                    let a = if String.IsNullOrEmpty acao then None else Some acao
                    let i = if inicio.HasValue then Some inicio.Value else None
                    let f = if fim.HasValue then Some fim.Value else None
                    let! logs = apiClient.GetAuditoriaAsync(t, ?utilizadorId = uId, ?acao = a, ?inicio = i, ?fim = f)
                    let logsList = System.Collections.Generic.List<_>(logs)
                    return this.View(logsList) :> IActionResult
                with
                | :? ApiClientException -> return this.RedirectToAction("Login", "Auth") :> IActionResult
        }

    [<HttpGet>]
    member this.Utilizadores () =
        task {
            let token = this.GetToken()
            match token with
            | null -> return this.RedirectToAction("Login", "Auth") :> IActionResult
            | t ->
                try
                    let! utilizadores = apiClient.ListUtilizadoresAsync(t)
                    let utilizadoresList = System.Collections.Generic.List<_>(utilizadores)
                    return this.View(utilizadoresList) :> IActionResult
                with
                | :? ApiClientException -> return this.RedirectToAction("Login", "Auth") :> IActionResult
        }

    [<HttpPost>]
    member this.AlterarRole (id: int, novaRole: string) =
        task {
            let token = this.GetToken()
            match token with
            | null -> return this.RedirectToAction("Login", "Auth") :> IActionResult
            | t ->
                try
                    let dto = RoleChangeDto(NovaRole = novaRole)
                    let! success = apiClient.AlterarRoleAsync(id, dto, t)
                    if success then
                        this.TempData["SuccessMessage"] <- "Role atualizada com sucesso!"
                    else
                        this.TempData["WarningMessage"] <- "Não foi possível atualizar a role."
                    return this.RedirectToAction("Utilizadores") :> IActionResult
                with
                | :? ApiClientException -> return this.RedirectToAction("Login", "Auth") :> IActionResult
        }