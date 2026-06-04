namespace ECHA.Web.Controllers

open Microsoft.AspNetCore.Mvc
open Microsoft.AspNetCore.Authorization
open System.Threading.Tasks
open ECHA.Core.DTOs
open Microsoft.AspNetCore.Http

[<Authorize(Roles = "Admin")>]
type EscolasController (apiClient: ECHA.Web.Services.ApiClient) =
    inherit Controller()

    private member this.GetToken() =
        this.User.FindFirst("AccessToken")?.Value

    [<HttpGet>]
    member this.Index () =
        task {
            let token = this.GetToken()
            match token with
            | null -> return this.Unauthorized() :> IActionResult
            | t ->
                let! escolas = apiClient.ListEscolasAsync(t)
                return this.View(escolas) :> IActionResult
        }

    [<HttpGet>]
    member this.Create () =
        this.View()

    [<HttpPost>]
    member this.Create (request: CreateEscolaDto) =
        task {
            let token = this.GetToken()
            match token with
            | null -> return this.Unauthorized() :> IActionResult
            | t ->
                let! success = apiClient.CreateEscolaAsync(request, t)
                if success then return this.RedirectToAction("Index") :> IActionResult
                else return this.View(request) :> IActionResult
        }

    [<HttpPost>]
    member this.GerarConvite (id: int) =
        task {
            let token = this.GetToken()
            match token with
            | null -> return this.Unauthorized() :> IActionResult
            | t ->
                let! result = apiClient.GerarCodigoConviteAsync(id, t)
                return this.RedirectToAction("Index") :> IActionResult
        }
