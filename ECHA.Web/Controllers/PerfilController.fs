namespace ECHA.Web.Controllers

open Microsoft.AspNetCore.Mvc
open Microsoft.AspNetCore.Authorization
open System.Threading.Tasks
open ECHA.Web.Services

[<Authorize>]
type PerfilController (apiClient: ApiClient) =
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
                // Assuming we get user details via API call based on token
                // Adjust based on actual API implementation
                return this.View() :> IActionResult
        }
