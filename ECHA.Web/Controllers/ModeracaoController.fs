namespace ECHA.Web.Controllers

open Microsoft.AspNetCore.Mvc
open Microsoft.AspNetCore.Authorization
open System
open System.Collections.Generic
open System.Threading.Tasks
open EconomiaComHistoria.Core.DTOs
open Microsoft.AspNetCore.Http

[<Authorize(Roles = "Editor,Admin")>]
type ModeracaoController (apiClient: ECHA.Web.Services.ApiClient) =
    inherit Controller()

    member private this.GetToken() =
        let claim = this.User.FindFirst("AccessToken")
        if isNull claim then null else claim.Value

    [<HttpGet>]
    member this.Fila () =
        task {
            let token = this.GetToken()
            match token with
            | null -> return this.Unauthorized() :> IActionResult
            | t ->
                let! pendentes = apiClient.GetPendentesAsync(t)
                match pendentes with
                | Some p -> return this.View(p) :> IActionResult
                | None -> return this.View(ModeracaoPendentesResponse(List<ModeracaoPendenteDto>(), List<ModeracaoPendenteDto>())) :> IActionResult
        }

    [<HttpGet>]
    member this.Denuncias () =
        task {
            let token = this.GetToken()
            match token with
            | null -> return this.Unauthorized() :> IActionResult
            | t ->
                let! denuncias = apiClient.GetDenunciasAsync(t)
                return this.View(denuncias) :> IActionResult
        }

    [<HttpGet>]
    member this.Utilizadores () =
        task {
            let token = this.GetToken()
            match token with
            | null -> return this.Unauthorized() :> IActionResult
            | t ->
                let! utilizadores = apiClient.ListUtilizadoresAsync(t)
                return this.View(utilizadores) :> IActionResult
        }

    [<HttpPost>]
    member this.AprovarTopico (id: int) =
        task {
            let token = this.GetToken()
            match token with
            | null -> return this.Unauthorized() :> IActionResult
            | t ->
                let! success = apiClient.AprovarTopicoAsync(id, t)
                return this.RedirectToAction("Fila") :> IActionResult
        }

    [<HttpPost>]
    member this.RejeitarTopico (id: int, motivo: string) =
        task {
            let token = this.GetToken()
            match token with
            | null -> return this.Unauthorized() :> IActionResult
            | t ->
                let request = RejeitarTopicoDto(motivo)
                let! success = apiClient.RejeitarTopicoAsync(id, request, t)
                return this.RedirectToAction("Fila") :> IActionResult
        }

    [<HttpPost>]
    member this.AprovarResposta (id: int) =
        task {
            let token = this.GetToken()
            match token with
            | null -> return this.Unauthorized() :> IActionResult
            | t ->
                let! success = apiClient.AprovarRespostaAsync(id, t)
                return this.RedirectToAction("Fila") :> IActionResult
        }

    [<HttpPost>]
    member this.RejeitarResposta (id: int, motivo: string) =
        task {
            let token = this.GetToken()
            match token with
            | null -> return this.Unauthorized() :> IActionResult
            | t ->
                let request = RejeitarTopicoDto(motivo)
                let! success = apiClient.RejeitarRespostaAsync(id, request, t)
                return this.RedirectToAction("Fila") :> IActionResult
        }

    [<HttpPost>]
    member this.SuspenderUtilizador (id: int, dias: System.Nullable<int>) =
        task {
            let token = this.GetToken()
            match token with
            | null -> return this.Unauthorized() :> IActionResult
            | t ->
                let d = if dias.HasValue then Some (Nullable(dias.Value)) else None
                let request = SuspenderUtilizadorDto(if dias.HasValue then Nullable(dias.Value) else Nullable())
                let! success = apiClient.SuspenderUtilizadorAsync(id, request, t)
                return this.RedirectToAction("Utilizadores") :> IActionResult
        }

    [<HttpPost>]
    member this.ReativarUtilizador (id: int) =
        task {
            let token = this.GetToken()
            match token with
            | null -> return this.Unauthorized() :> IActionResult
            | t ->
                let! success = apiClient.ReativarUtilizadorAsync(id, t)
                return this.RedirectToAction("Utilizadores") :> IActionResult
        }
