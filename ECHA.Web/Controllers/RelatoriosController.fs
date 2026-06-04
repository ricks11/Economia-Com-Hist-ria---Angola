namespace ECHA.Web.Controllers

open Microsoft.AspNetCore.Mvc
open Microsoft.AspNetCore.Authorization
open System.Threading.Tasks
open ECHA.Core.DTOs
open Microsoft.AspNetCore.Http

[<Authorize(Roles = "Editor,Admin")>]
type RelatoriosController (apiClient: ECHA.Web.Services.ApiClient) =
    inherit Controller()

    private member this.GetToken() =
        this.User.FindFirst("AccessToken")?.Value

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
                | Some s -> return this.RedirectToAction("Status", new { id = s.Id }) :> IActionResult
                | None -> return this.BadRequest("Falha ao solicitar relatório") :> IActionResult
        }

    [<HttpGet>]
    member this.Status (id: int) =
        task {
            // Seria implementado GetStatus no ApiClient
            return this.View() :> IActionResult
        }
