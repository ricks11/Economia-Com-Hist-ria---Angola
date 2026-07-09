namespace ECHA.Web.Controllers

open Microsoft.AspNetCore.Mvc
open Microsoft.AspNetCore.Authorization
open System
open System.Threading.Tasks
open ECHA.Web.Services

[<Authorize>]
type RankingsController(apiClient: ApiClient) =
    inherit Controller()

    member private this.GetToken() =
        let claim = this.User.FindFirst("AccessToken")
        if isNull claim then null else claim.Value

    [<HttpGet>]
    member this.Index(tipo: string, periodo: string) =
        task {
            let token = this.GetToken()
            match token with
            | null -> return this.RedirectToAction("Login", "Auth") :> IActionResult
            | t ->
                try
                    let tipoParam = if String.IsNullOrEmpty tipo then "Nacional" else tipo
                    let periodoParam = if String.IsNullOrEmpty periodo then "Semanal" else periodo
                    let! ranking = apiClient.GetRankingAsync(tipoParam, periodoParam, t)
                    match ranking with
                    | Some r -> return this.View(r) :> IActionResult
                    | None -> return this.View() :> IActionResult
                with
                | :? ApiClientException ->
                    return this.RedirectToAction("Login", "Auth") :> IActionResult
        }
