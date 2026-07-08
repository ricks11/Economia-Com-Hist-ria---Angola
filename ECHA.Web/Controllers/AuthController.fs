namespace ECHA.Web.Controllers

open Microsoft.AspNetCore.Mvc
open System.Threading.Tasks
open EconomiaComHistoria.Core.DTOs
open Microsoft.AspNetCore.Authentication
open System.Security.Claims
open System

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
        task {
            if String.IsNullOrWhiteSpace(email) then
                this.ModelState.AddModelError("", "Por favor, insira um email válido.")
                return this.View() :> IActionResult
            else
                let! success = apiClient.ForgotPasswordAsync(email)
                if success then
                    return this.View("ForgotPasswordConfirmation") :> IActionResult
                else
                    this.ModelState.AddModelError("", "A API backend falhou ao processar o pedido.")
                    return this.View() :> IActionResult
        }

    [<HttpPost>]
    member this.Logout() =
        task {
            // Garante que o SignOut também limpa o cookie correto
            do! this.HttpContext.SignOutAsync(AuthScheme)
            return this.RedirectToAction("Index", "Home") :> IActionResult
        }

    [<HttpGet>]
    member this.ResetPassword() =
        // Lemos diretamente os parâmetros da Query String do pedido HTTP
        let query = this.Request.Query
        let token = if query.ContainsKey("token") then query.["token"].ToString() else ""
        let email = if query.ContainsKey("email") then query.["email"].ToString() else ""

        if String.IsNullOrWhiteSpace(token) || String.IsNullOrWhiteSpace(email) then
            // Se o link vier sem parâmetros válidos, manda de volta para o Login
            this.RedirectToAction("Login") :> IActionResult
        else
            // Injeta os valores na ViewData para serem renderizados no HTML
            this.ViewData["Token"] <- token
            this.ViewData["Email"] <- email
            this.View() :> IActionResult

    [<HttpPost>]
    member this.ResetPassword(form: Microsoft.AspNetCore.Http.IFormCollection) =
        task {
            // Capturamos os valores vindos do formulário HTML de forma explícita
            let email = form.["Email"].ToString()
            let token = form.["Token"].ToString()
            let newPassword = form.["NewPassword"].ToString()

            if String.IsNullOrWhiteSpace(email) || String.IsNullOrWhiteSpace(newPassword) then
                this.ModelState.AddModelError("", "Dados de recuperação inválidos.")
                return this.View() :> IActionResult
            else
                // Instanciamos o DTO explicitamente com os valores capturados
                let dto = EconomiaComHistoria.Core.DTOs.ResetPasswordRequestDto(email, token, newPassword)
            
                let! success = apiClient.ResetPasswordAsync(dto)
                if success then
                    this.TempData["SuccessMessage"] <- "Palavra-passe redefinida com sucesso! Faça login com a nova credencial."
                    return this.RedirectToAction("Login") :> IActionResult
                else
                    this.ModelState.AddModelError("", "Não foi possível redefinir a palavra-passe. O link pode ter expirado ou os dados são inválidos.")
                    this.ViewData["Token"] <- token
                    this.ViewData["Email"] <- email
                    return this.View() :> IActionResult
        }