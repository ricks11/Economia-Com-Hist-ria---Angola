namespace ECHA.Web.Controllers

open Microsoft.AspNetCore.Mvc
open Microsoft.AspNetCore.Authorization
open System.Threading.Tasks
open EconomiaComHistoria.Core.DTOs
open Microsoft.AspNetCore.Http

[<Authorize(Roles = "Editor,Admin")>]
type ModeracaoController (apiClient: ECHA.Web.Services.ApiClient) =
    inherit Controller()

    private member this.GetToken() =
        this.User.FindFirst("AccessToken")?.Value

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
                | None -> return this.View(ModeracaoPendentesResponse([], [])) :> IActionResult
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
                let request = { MotivoRejeicao = motivo }
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
                let request = { MotivoRejeicao = motivo }
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
                let d = if dias.HasValue then Some dias.Value else None
                let request = { DiasSuspensao = d; Motivo = "Violação de termos" }
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
