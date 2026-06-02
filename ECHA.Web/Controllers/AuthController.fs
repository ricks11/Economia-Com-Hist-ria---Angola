namespace ECHA.Web.Controllers

open Microsoft.AspNetCore.Mvc
open System.Threading.Tasks
open ECHA.Core.DTOs

open Microsoft.AspNetCore.Authentication
open Microsoft.AspNetCore.Authentication.Cookies
open System.Security.Claims

type AuthController (apiClient: ECHA.Web.Services.ApiClient) =
    inherit Controller()

    [<HttpGet>]
    member this.Login () =
        this.View()

    [<HttpPost>]
    member this.Login (request: LoginRequestDto) =
        task {
            let! token = apiClient.LoginAsync(request)
            match token with
            | Some jwtToken ->
                let claims = [| Claim(ClaimTypes.Name, request.Email) |]
                let claimsIdentity = ClaimsIdentity(claims, "CookieAuthentication")
                let claimsPrincipal = ClaimsPrincipal(claimsIdentity)
                
                do! this.HttpContext.SignInAsync("CookieAuthentication", claimsPrincipal)
                return this.RedirectToAction("Index", "Home") :> IActionResult
            | None ->
                this.ModelState.AddModelError("", "Login failed")
                return this.View(request) :> IActionResult
        }
