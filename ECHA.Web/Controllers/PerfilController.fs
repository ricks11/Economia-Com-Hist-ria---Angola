namespace EconomiaComHistoria.Web.Controllers

open Microsoft.AspNetCore.Mvc
open Microsoft.AspNetCore.Authorization
open Microsoft.AspNetCore.Authentication
open System.Threading.Tasks
open ECHA.Web.Services

type PerfilController(apiClient: ApiClient) =
    inherit Controller()

    [<HttpGet>]
    member this.Index() =
        task {
            // Vai buscar a claim onde guardaste o JWT
            let tokenClaim = this.User.FindFirst("AccessToken")
            
            if isNull tokenClaim || System.String.IsNullOrEmpty(tokenClaim.Value) then
                return this.RedirectToAction("Login", "Auth") :> IActionResult
            else
                // REMOVE POSSÍVEIS ASPAS EMBUTIDAS QUE FAZEM O BEARER FALHAR
                let token = tokenClaim.Value.Trim().Replace("\"", "")
                
                try
                    // Faz o pedido à API através do ApiClient
                    let! perfilResult = apiClient.GetPerfilAsync(token)
                    
                    match perfilResult with
                    | Some perfil -> 
                        return this.View(perfil) :> IActionResult
                    | None -> 
                        return this.NotFound("Não foi possível encontrar os dados do perfil.") :> IActionResult
                with
                | :? ApiClientException as ex when (int ex.StatusCode = 401) ->
                    // Se a API disser que o token é inválido/expirou, manda de volta para o Login
                    return this.RedirectToAction("Login", "Auth") :> IActionResult
        }