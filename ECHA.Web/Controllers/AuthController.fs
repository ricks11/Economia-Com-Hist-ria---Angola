namespace ECHA.Web.Controllers

open Microsoft.AspNetCore.Mvc
open System.Threading.Tasks
open EconomiaComHistoria.Core.DTOs
open Microsoft.AspNetCore.Authentication
open System.Security.Claims

type AuthController(apiClient: ECHA.Web.Services.ApiClient) =
    inherit Controller()

    // Definimos uma constante local para evitar repetir a string mágica
    [<Literal>]
    let AuthScheme = "CookieAuthentication"

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
                // Mudado aqui para usar o "CookieAuthentication" definido no Program.fs
                let claimsIdentity = ClaimsIdentity(claims, AuthScheme)
                let claimsPrincipal = ClaimsPrincipal(claimsIdentity)
                
                do! this.HttpContext.SignInAsync(AuthScheme, claimsPrincipal)
                return this.RedirectToAction("Index", "Home") :> IActionResult
            | None ->
                this.ModelState.AddModelError("", "Login failed")
                return this.View(request) :> IActionResult
        }

    [<HttpPost>]
    member this.Register(request: RegisterRequestDto) =
        task {
            let! authResponse = apiClient.RegisterAsync(request)
            match authResponse with
            | Some response ->
                let claims =
                    [| Claim(ClaimTypes.Name, response.Email)
                       Claim(ClaimTypes.Role, response.Tipo)
                       Claim("AccessToken", response.AccessToken) |]
                // Mudado aqui também para o registo automático
                let claimsIdentity = ClaimsIdentity(claims, AuthScheme)
                let claimsPrincipal = ClaimsPrincipal(claimsIdentity)
                
                do! this.HttpContext.SignInAsync(AuthScheme, claimsPrincipal)
                return this.RedirectToAction("Index", "Home") :> IActionResult
            | None ->
                this.ModelState.AddModelError("", "Falha ao criar conta. Verifique os dados ou se o email já existe.")
                this.ViewData["ActiveTab"] <- "register"
                return this.View("Login") :> IActionResult
            }

    [<HttpGet>]
    member this.ForgotPassword() =
        this.View()

    [<HttpPost>]
    member this.ForgotPassword(email: string) =
        this.View("ForgotPasswordConfirmation")

    [<HttpPost>]
    member this.Logout() =
        task {
            // Garante que o SignOut também limpa o cookie correto
            do! this.HttpContext.SignOutAsync(AuthScheme)
            return this.RedirectToAction("Index", "Home") :> IActionResult
        }