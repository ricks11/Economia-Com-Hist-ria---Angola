namespace ECHA.Web.Controllers

open Microsoft.AspNetCore.Mvc
open Microsoft.AspNetCore.Authorization
open System
open System.Threading.Tasks
open EconomiaComHistoria.Core.DTOs
open Microsoft.AspNetCore.Http

[<Authorize(Roles = "Admin,SuperAdmin")>]
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
                if success then
                    this.TempData["SuccessMessage"] <- "Escola criada com sucesso!"
                    return this.RedirectToAction("Index") :> IActionResult
                else
                    this.TempData["ErrorMessage"] <- "Erro ao criar escola."
                    return this.View(request) :> IActionResult
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
                if result.IsSome then
                    this.TempData["SuccessMessage"] <- "Escola atualizada com sucesso!"
                    return this.RedirectToAction("Index") :> IActionResult
                else
                    this.TempData["ErrorMessage"] <- "Erro ao atualizar escola."
                    return this.View(request) :> IActionResult
        }

    [<HttpPost>]
    member this.Delete (id: int) =
        task {
            let token = this.GetToken()
            match token with
            | null -> return this.Unauthorized() :> IActionResult
            | t ->
                let! success = apiClient.DeleteEscolaAsync(id, t)
                if success then
                    this.TempData["SuccessMessage"] <- "Escola eliminada com sucesso."
                else
                    this.TempData["ErrorMessage"] <- "Erro ao eliminar escola."
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
                match result with
                | Some invite ->
                    this.TempData["SuccessMessage"] <- $"Novo código de convite gerado: {invite.Codigo}"
                | None ->
                    this.TempData["ErrorMessage"] <- "Erro ao gerar código de convite."
                return this.RedirectToAction("Index") :> IActionResult
        }

    [<HttpPost>]
    member this.RevogarConvite (id: int) =
        task {
            let token = this.GetToken()
            match token with
            | null -> return this.Unauthorized() :> IActionResult
            | t ->
                // Como o ApiClient não tem método Revogar, podemos fazer um DELETE para o mesmo endpoint
                // Vamos adicionar no ApiClient
                let! success = apiClient.RevogarCodigoConviteAsync(id, t)
                if success then
                    this.TempData["SuccessMessage"] <- "Código de convite revogado."
                else
                    this.TempData["ErrorMessage"] <- "Erro ao revogar código."
                return this.RedirectToAction("Index") :> IActionResult
        }