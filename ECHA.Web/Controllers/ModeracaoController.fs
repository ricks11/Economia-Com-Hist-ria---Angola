namespace ECHA.Web.Controllers

open System
open System.Collections.Generic
open System.Threading.Tasks
open Microsoft.AspNetCore.Mvc
open Microsoft.AspNetCore.Authorization
open Microsoft.AspNetCore.Http
open EconomiaComHistoria.Core.DTOs

[<Authorize(Roles = "Admin,Editor,Moderador,SuperAdmin")>]
type ModeracaoController (apiClient: ECHA.Web.Services.ApiClient) =
    inherit Controller()

    member private this.GetToken() =
        let claim = this.User.FindFirst("AccessToken")
        if isNull claim then null else claim.Value

    member private this.RedirectToRefererOrAction (defaultAction: string) =
        let referer = this.Request.Headers.["Referer"].ToString()
        if not (String.IsNullOrEmpty(referer)) then
            this.Redirect(referer) :> IActionResult
        else
            this.RedirectToAction(defaultAction) :> IActionResult

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
                | None -> 
                    // CORREÇÃO: Passar instâncias vazias de System.Collections.Generic.List
                    let listaVazia1 = System.Collections.Generic.List<ModeracaoPendenteDto>()
                    let listaVazia2 = System.Collections.Generic.List<ModeracaoPendenteDto>()
                    return this.View(ModeracaoPendentesResponse(listaVazia1, listaVazia2)) :> IActionResult
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
                let! _ = apiClient.AprovarTopicoAsync(id, t)
                return this.RedirectToRefererOrAction("Fila")
        }

    [<HttpPost>]
    member this.RejeitarTopico (id: int, motivo: string) =
        task {
            let token = this.GetToken()
            match token with
            | null -> return this.Unauthorized() :> IActionResult
            | t ->
                let request = RejeitarTopicoDto(motivo)
                let! _ = apiClient.RejeitarTopicoAsync(id, request, t)
                return this.RedirectToRefererOrAction("Fila")
        }

    [<HttpPost>]
    member this.AprovarResposta (id: int) =
        task {
            let token = this.GetToken()
            match token with
            | null -> return this.Unauthorized() :> IActionResult
            | t ->
                let! _ = apiClient.AprovarRespostaAsync(id, t)
                return this.RedirectToRefererOrAction("Fila")
        }

    [<HttpPost>]
    member this.RejeitarResposta (id: int, motivo: string) =
        task {
            let token = this.GetToken()
            match token with
            | null -> return this.Unauthorized() :> IActionResult
            | t ->
                let request = RejeitarTopicoDto(motivo)
                let! _ = apiClient.RejeitarRespostaAsync(id, request, t)
                return this.RedirectToRefererOrAction("Fila")
        }

    [<HttpPost>]
    member this.SuspenderUtilizador (id: int, dias: Nullable<int>) =
        task {
            let token = this.GetToken()
            match token with
            | null -> return this.Unauthorized() :> IActionResult
            | t ->
                let request = SuspenderUtilizadorDto(dias)
                let! _ = apiClient.SuspenderUtilizadorAsync(id, request, t)
                return this.RedirectToAction("Utilizadores") :> IActionResult
        }

    [<HttpPost>]
    member this.ReativarUtilizador (id: int) =
        task {
            let token = this.GetToken()
            match token with
            | null -> return this.Unauthorized() :> IActionResult
            | t ->
                let! _ = apiClient.ReativarUtilizadorAsync(id, t)
                return this.RedirectToAction("Utilizadores") :> IActionResult
        }