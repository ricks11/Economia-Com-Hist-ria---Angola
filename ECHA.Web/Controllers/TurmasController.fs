namespace ECHA.Web.Controllers

open Microsoft.AspNetCore.Mvc
open Microsoft.AspNetCore.Authorization
open System
open System.Threading.Tasks
open EconomiaComHistoria.Core.DTOs
open Microsoft.AspNetCore.Http

[<Authorize(Roles = "Editor,Admin")>]
type TurmasController (apiClient: ECHA.Web.Services.ApiClient) =
    inherit Controller()

    member private this.GetToken() =
        let claim = this.User.FindFirst("AccessToken")
        if isNull claim then null else claim.Value

    [<HttpGet>]
    member this.Index (escolaId: System.Nullable<int>) =
        task {
            let token = this.GetToken()
            match token with
            | null -> return this.Unauthorized() :> IActionResult
            | t ->
                let eId = if escolaId.HasValue then Some escolaId.Value else None
                let! turmas = apiClient.ListTurmasAsync(t, ?escolaId = eId)
                return this.View(turmas) :> IActionResult
        }

    [<HttpGet>]
    member this.Details (id: int) =
        task {
            let token = this.GetToken()
            match token with
            | null -> return this.Unauthorized() :> IActionResult
            | t ->
                let! turma = apiClient.GetTurmaDetalheAsync(id, t)
                match turma with
                | Some d -> return this.View(d) :> IActionResult
                | None -> return this.NotFound() :> IActionResult
        }

    [<HttpGet>]
    member this.Create (escolaId: int) =
        this.View(new CreateTurmaDto("", System.Nullable<int>(), escolaId, System.Nullable<int>()))

    [<HttpPost>]
    member this.Create (request: CreateTurmaDto) =
        task {
            let token = this.GetToken()
            match token with
            | null -> return this.Unauthorized() :> IActionResult
            | t ->
                let! success = apiClient.CreateTurmaAsync(request, t)
                if success then return this.RedirectToAction("Index", {| escolaId = request.EscolaId |}) :> IActionResult
                else return this.View(request) :> IActionResult
        }
