namespace ECHA.Web.Controllers

open Microsoft.AspNetCore.Mvc
open Microsoft.AspNetCore.Authorization
open System.Threading.Tasks
open EconomiaComHistoria.Core.DTOs
open ECHA.Web.Services
open ECHA.Web.Models

[<Authorize>]
type PerfilController(apiClient: ApiClient) =
    inherit Controller()

    member private this.GetToken() =
        let authHeader =
            this.HttpContext.Request.Headers["Authorization"]
            |> Seq.tryHead
        match authHeader with
        | Some header when header.StartsWith("Bearer ") -> header.Substring("Bearer ".Length)
        | _ ->
            // fallback: tries the old claim
            let claim = this.User.FindFirst("AccessToken")
            if isNull claim then null else claim.Value

    [<HttpGet>]
    member this.Index(tab: string) =
        task {
            let token = this.GetToken()
            match token with
            | null -> return this.RedirectToAction("Login", "Auth") :> IActionResult
            | t ->
                try
                    let! perfilOpt = apiClient.GetPerfilAsync(t)
                    match perfilOpt with
                    | None -> return this.NotFound() :> IActionResult
                    | Some perfil ->
                        let! progresso = apiClient.GetProgressoAsync(t)
                        let! escolas = apiClient.ListEscolasAsync(t)   // Agora filtrado pela API
                        let! turmas = apiClient.ListTurmasAsync(t)     // Agora filtrado pela API
                        let! favoritos = apiClient.ListFavoritosAsync(t)

                        let vm = {
                            Perfil = perfil
                            Progresso = progresso
                            Escolas = escolas
                            Turmas = turmas
                            Favoritos = favoritos
                            TabAtiva = if System.String.IsNullOrEmpty tab then "perfil" else tab
                        }
                        return this.View(vm) :> IActionResult
                with
                | :? ApiClientException ->
                    return this.RedirectToAction("Login", "Auth") :> IActionResult
        }

    [<HttpPost>]
    member this.Atualizar(request: UpdatePerfilDto) =
        task {
            let token = this.GetToken()
            match token with
            | null -> return this.RedirectToAction("Login", "Auth") :> IActionResult
            | t ->
                try
                    let! sucesso = apiClient.UpdatePerfilAsync(request, t)
                    if sucesso then
                        this.TempData["SuccessMessage"] <- "Perfil actualizado com sucesso!"
                    else
                        this.TempData["ErrorMessage"] <- "Não foi possível actualizar o perfil."
                    return this.RedirectToAction("Index") :> IActionResult
                with
                | :? ApiClientException ->
                    return this.RedirectToAction("Login", "Auth") :> IActionResult
        }