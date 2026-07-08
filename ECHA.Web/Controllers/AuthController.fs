namespace ECHA.Web.Controllers

open Microsoft.AspNetCore.Mvc
open System.Threading.Tasks
open EconomiaComHistoria.Core.DTOs
open Microsoft.AspNetCore.Authentication
open Microsoft.AspNetCore.Authentication.Cookies
open System.Security.Claims

type AuthController(apiClient: ECHA.Web.Services.ApiClient) =
    inherit Controller()

    [<HttpGet>]
    member this.Login() =
        this.View()

    [<HttpPost>]
    member this.Login(request: LoginRequestDto) =
        task {
            let! authResponse = apiClient.LoginAsync(request)
            match authResponse with
            | Some response ->
                let claims =
                    [| Claim(ClaimTypes.Name, response.Email)
                       Claim(ClaimTypes.Role, response.Tipo)
                       Claim("AccessToken", response.AccessToken) |]
                let claimsIdentity = ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme)
                let claimsPrincipal = ClaimsPrincipal(claimsIdentity)
                do! this.HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal)
                return this.RedirectToAction("Index", "Home") :> IActionResult
            | None ->
                this.ModelState.AddModelError("", "Login failed")
                return this.View(request) :> IActionResult
        }

    [<HttpGet>]
    member this.ForgotPassword() =
        this.View()

    [<HttpPost>]
    member this.ForgotPassword(email: string) =
        // In a real app, we would send a reset email here
        // For now, just show a success message (whether email exists or not - for security)
        this.View("ForgotPasswordConfirmation")

    [<HttpPost>]
    member this.Logout() =
        task {
            do! this.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme)
            return this.RedirectToAction("Index", "Home") :> IActionResult
        }
