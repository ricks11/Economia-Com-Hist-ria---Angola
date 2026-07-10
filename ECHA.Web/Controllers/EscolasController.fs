namespace ECHA.Web.Controllers

open Microsoft.AspNetCore.Mvc
open Microsoft.AspNetCore.Authorization
open System
open System.Threading.Tasks
open EconomiaComHistoria.Core.DTOs
open Microsoft.AspNetCore.Http

[<Authorize(Roles = "Admin")>]
type EscolasController (apiClient: ECHA.Web.Services.ApiClient) =
    inherit Controller()

    member private this.GetToken() =
        let claim = this.User.FindFirst("AccessToken")
        if isNull claim then null else claim.Value

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

    [<HttpGet>]
    member this.Edit (id: int) =
        task {
            let token = this.GetToken()
            match token with
            | null -> return this.Unauthorized() :> IActionResult
            | t ->
                let! escola = apiClient.GetEscolaAsync(id, t)
                match escola with
                | Some e -> return this.View(e) :> IActionResult
                | None -> return this.NotFound() :> IActionResult
        }

    [<HttpPost>]
    member this.Edit (id: int, request: CreateEscolaDto) =
        task {
            let token = this.GetToken()
            match token with
            | null -> return this.Unauthorized() :> IActionResult
            | t ->
                let! result = apiClient.UpdateEscolaAsync(id, request, t)
                if result.IsSome then return this.RedirectToAction("Index") :> IActionResult
                else return this.View(request) :> IActionResult
        }

    [<HttpPost>]
    member this.Delete (id: int) =
        task {
            let token = this.GetToken()
            match token with
            | null -> return this.Unauthorized() :> IActionResult
            | t ->
                let! success = apiClient.DeleteEscolaAsync(id, t)
                return this.RedirectToAction("Index") :> IActionResult
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
