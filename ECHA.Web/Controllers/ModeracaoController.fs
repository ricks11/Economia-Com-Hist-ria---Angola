namespace ECHA.Web.Controllers

open System
open System.Collections.Generic
open System.Threading.Tasks
open Microsoft.AspNetCore.Mvc
open Microsoft.AspNetCore.Authorization
open Microsoft.AspNetCore.Http
open EconomiaComHistoria.Core.DTOs
open ECHA.Web.Services

[<Authorize(Roles = "Admin,Editor,Moderador,SuperAdmin")>]
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
                | None -> 
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
    [<ValidateAntiForgeryToken>]
    member this.AprovarTopico (id: int) =
        task {
            let token = this.GetToken()
            match token with
            | null -> 
                this.TempData["ErrorMessage"] <- "Sessão expirada. Faça login novamente."
                return this.RedirectToAction("Login", "Auth") :> IActionResult
            | t ->
                try
                    let! sucesso = apiClient.AprovarTopicoAsync(id, t)
                    if sucesso then
                        this.TempData["SuccessMessage"] <- "Tópico aprovado com sucesso."
                    else
                        this.TempData["ErrorMessage"] <- "Falha ao aprovar tópico."
                with
                | :? ApiClientException as ex ->
                    this.TempData["ErrorMessage"] <- $"Erro: {ex.Message}"
                return this.RedirectToAction("Denuncias") :> IActionResult
        }

    [<HttpPost>]
    [<ValidateAntiForgeryToken>]
    member this.RejeitarTopico (id: int, motivo: string) =
        task {
            let token = this.GetToken()
            match token with
            | null -> 
                this.TempData["ErrorMessage"] <- "Sessão expirada. Faça login novamente."
                return this.RedirectToAction("Login", "Auth") :> IActionResult
            | t ->
                try
                    let request = RejeitarTopicoDto(motivo)
                    let! sucesso = apiClient.RejeitarTopicoAsync(id, request, t)
                    if sucesso then
                        this.TempData["SuccessMessage"] <- "Tópico rejeitado e penalização aplicada."
                    else
                        this.TempData["ErrorMessage"] <- "Falha ao rejeitar tópico."
                with
                | :? ApiClientException as ex ->
                    this.TempData["ErrorMessage"] <- $"Erro: {ex.Message}"
                return this.RedirectToAction("Denuncias") :> IActionResult
        }

    [<HttpPost>]
    [<ValidateAntiForgeryToken>]
    member this.AprovarResposta (id: int) =
        task {
            let token = this.GetToken()
            match token with
            | null -> 
                this.TempData["ErrorMessage"] <- "Sessão expirada. Faça login novamente."
                return this.RedirectToAction("Login", "Auth") :> IActionResult
            | t ->
                try
                    let! sucesso = apiClient.AprovarRespostaAsync(id, t)
                    if sucesso then
                        this.TempData["SuccessMessage"] <- "Resposta aprovada com sucesso."
                    else
                        this.TempData["ErrorMessage"] <- "Falha ao aprovar resposta."
                with
                | :? ApiClientException as ex ->
                    this.TempData["ErrorMessage"] <- $"Erro: {ex.Message}"
                return this.RedirectToAction("Denuncias") :> IActionResult
        }

    [<HttpPost>]
    [<ValidateAntiForgeryToken>]
    member this.RejeitarResposta (id: int, motivo: string) =
        task {
            let token = this.GetToken()
            match token with
            | null -> 
                this.TempData["ErrorMessage"] <- "Sessão expirada. Faça login novamente."
                return this.RedirectToAction("Login", "Auth") :> IActionResult
            | t ->
                try
                    let request = RejeitarTopicoDto(motivo)
                    let! sucesso = apiClient.RejeitarRespostaAsync(id, request, t)
                    if sucesso then
                        this.TempData["SuccessMessage"] <- "Resposta rejeitada e penalização aplicada."
                    else
                        this.TempData["ErrorMessage"] <- "Falha ao rejeitar resposta."
                with
                | :? ApiClientException as ex ->
                    this.TempData["ErrorMessage"] <- $"Erro: {ex.Message}"
                return this.RedirectToAction("Denuncias") :> IActionResult
        }

    [<HttpPost>]
    [<ValidateAntiForgeryToken>]
    member this.SuspenderUtilizador (id: int, dias: Nullable<int>) =
        task {
            let token = this.GetToken()
            match token with
            | null -> 
                this.TempData["ErrorMessage"] <- "Sessão expirada."
                return this.RedirectToAction("Login", "Auth") :> IActionResult
            | t ->
                try
                    let request = SuspenderUtilizadorDto(dias)
                    let! sucesso = apiClient.SuspenderUtilizadorAsync(id, request, t)
                    if sucesso then
                        this.TempData["SuccessMessage"] <- "Utilizador suspenso com sucesso."
                    else
                        this.TempData["ErrorMessage"] <- "Falha ao suspender utilizador."
                with
                | :? ApiClientException as ex ->
                    this.TempData["ErrorMessage"] <- $"Erro: {ex.Message}"
                return this.RedirectToAction("Utilizadores") :> IActionResult
        }

    [<HttpPost>]
    [<ValidateAntiForgeryToken>]
    member this.ReativarUtilizador (id: int) =
        task {
            let token = this.GetToken()
            match token with
            | null -> 
                this.TempData["ErrorMessage"] <- "Sessão expirada."
                return this.RedirectToAction("Login", "Auth") :> IActionResult
            | t ->
                try
                    let! sucesso = apiClient.ReativarUtilizadorAsync(id, t)
                    if sucesso then
                        this.TempData["SuccessMessage"] <- "Utilizador reativado com sucesso."
                    else
                        this.TempData["ErrorMessage"] <- "Falha ao reativar utilizador."
                with
                | :? ApiClientException as ex ->
                    this.TempData["ErrorMessage"] <- $"Erro: {ex.Message}"
                return this.RedirectToAction("Utilizadores") :> IActionResult
        }