namespace ECHA.Web.Controllers

open Microsoft.AspNetCore.Mvc
open Microsoft.AspNetCore.Authorization
open System
open System.Threading.Tasks
open EconomiaComHistoria.Core.DTOs
open Microsoft.AspNetCore.Http

[<Authorize(Roles = "Editor,Admin")>]
type RelatoriosController (apiClient: ECHA.Web.Services.ApiClient) =
    inherit Controller()

    member private this.GetToken() =
        let claim = this.User.FindFirst("AccessToken")
        if isNull claim then null else claim.Value

    [<HttpGet>]
    member this.Index () =
        this.View()

    [<HttpPost>]
    member this.Gerar (request: SolicitarRelatorioDto) =
        task {
            let token = this.GetToken()
            match token with
            | null -> return this.Unauthorized() :> IActionResult
            | t ->
                let! status = apiClient.SolicitarRelatorioAsync(request, t)
                match status with
                | Some s -> return this.RedirectToAction("Status", {| id = s.Id |}) :> IActionResult
                | None -> return this.BadRequest("Falha ao solicitar relatório") :> IActionResult
        }

    [<HttpGet>]
    member this.Status (id: int) =
        task {
            let token = this.GetToken()
            match token with
            | null -> return this.Unauthorized() :> IActionResult
            | t ->
                let! status = apiClient.GetRelatorioStatusAsync(id, t)
                match status with
                | Some s -> return this.View(s) :> IActionResult
                | None -> return this.NotFound() :> IActionResult
        }
