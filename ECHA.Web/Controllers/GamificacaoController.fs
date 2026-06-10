namespace ECHA.Web.Controllers

open Microsoft.AspNetCore.Mvc
open Microsoft.AspNetCore.Authorization
open System
open System.Threading.Tasks
open EconomiaComHistoria.Core.DTOs
open Microsoft.AspNetCore.Http

[<Authorize>]
type GamificacaoController (apiClient: ECHA.Web.Services.ApiClient) =
    inherit Controller()

    member private this.GetToken() =
        let claim = this.User.FindFirst("AccessToken")
        if isNull claim then null else claim.Value

    [<HttpGet>]
    member this.MeuProgresso () =
        task {
            let token = this.GetToken()
            match token with
            | null -> return this.Unauthorized() :> IActionResult
            | t ->
                let! progresso = apiClient.GetProgressoAsync(t)
                match progresso with
                | Some p -> return this.View(p) :> IActionResult
                | None -> return this.NotFound() :> IActionResult
        }

    [<HttpPost>]
    member this.GerarPlanoEstudo () =
        task {
            let token = this.GetToken()
            match token with
            | null -> return this.Unauthorized() :> IActionResult
            | t ->
                let! success = apiClient.GerarPlanoEstudoAsync(t)
                return this.RedirectToAction("MeuProgresso") :> IActionResult
        }

    [<HttpGet>]
    [<Authorize(Roles = "Admin")>]
    member this.DashboardAdmin () =
        task {
            let token = this.GetToken()
            match token with
            | null -> return this.Unauthorized() :> IActionResult
            | t ->
                let! badges = apiClient.GetBadgesAsync(t)
                let! metricas = apiClient.GetMetricasEngajamentoAsync(t)
                this.ViewData.["Metricas"] <- metricas
                return this.View(badges) :> IActionResult
        }
