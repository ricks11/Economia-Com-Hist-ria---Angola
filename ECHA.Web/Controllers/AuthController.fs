namespace ECHA.Web.Controllers

open Microsoft.AspNetCore.Mvc
open System.Threading.Tasks
open EconomiaComHistoria.Core.DTOs
open Microsoft.AspNetCore.Authentication
open System.Security.Claims
open System
open ECHA.Web.Services

type AuthController(apiClient: ECHA.Web.Services.ApiClient) =
    inherit Controller()

    // Constante local correspondente ao esquema configurado no Program.fs do projeto Web
    [<Literal>]
    let AuthScheme = "CookieAuthentication"

    [<HttpGet>]
    member this.Login() =
        this.View()

    [<HttpPost>]
    member this.Login(request: LoginRequestDto) =
        task {
            try
                let! authResponse = apiClient.LoginAsync(request)
                match authResponse with
                | Some response ->
                    // 1. Procurar o utilizador na listagem de moderadores para verificar o estado de suspensão
                    let! utilizadores = apiClient.ListUtilizadoresAsync(response.AccessToken)
                
                    // Tenta encontrar o gajo que está a tentar fazer login pelo Email
                    let utilizadorLogado = 
                        utilizadores 
                        |> List.tryFind (fun u -> String.Equals(u.Email, response.Email, StringComparison.OrdinalIgnoreCase))

                    match utilizadorLogado with
                    | Some u when u.Suspenso ->
                        // Se o utilizador foi encontrado e está marcado como Suspenso/Banido, barra na hora!
                        this.ModelState.AddModelError("", "Esta conta encontra-se suspensa ou banida da plataforma.")
                        return this.View(request) :> IActionResult
                    
                    | _ ->
                        // 2. Se não estiver suspenso (ou não constar na lista de moderação), prossegue normalmente
                        let claims =
                            [| Claim(ClaimTypes.Name, response.Email)
                               Claim(ClaimTypes.Role, response.Tipo)
                               Claim("AccessToken", response.AccessToken) |]
                    
                        let claimsIdentity = ClaimsIdentity(claims, AuthScheme, ClaimTypes.Name, ClaimTypes.Role)
                        let claimsPrincipal = ClaimsPrincipal(claimsIdentity)
                    
                        let authProperties = AuthenticationProperties()
                        authProperties.StoreTokens([
                            AuthenticationToken(Name = "access_token", Value = response.AccessToken)
                        ])
                        authProperties.IsPersistent <- true 

                        do! this.HttpContext.SignInAsync(AuthScheme, claimsPrincipal, authProperties)
                        return this.RedirectToAction("Index", "Home") :> IActionResult
                | None ->
                    this.ModelState.AddModelError("", "Falha no início de sessão. Verifique as suas credenciais.")
                    return this.View(request) :> IActionResult
            with
            | :? ApiClientException as ex ->
                // Se o backend enviou uma mensagem explícita (como a de banimento), mostra ela na View!
                this.ModelState.AddModelError("", ex.Message)
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
                
                // CORREÇÃO: Aplicado o mesmo mapeamento de Roles para o registo automático
                let claimsIdentity = ClaimsIdentity(claims, AuthScheme, ClaimTypes.Name, ClaimTypes.Role)
                let claimsPrincipal = ClaimsPrincipal(claimsIdentity)

                // Configuração das propriedades de autenticação para armazenar o token JWT no Cookie
                let authProperties = AuthenticationProperties()
                authProperties.StoreTokens([
                    AuthenticationToken(Name = "access_token", Value = response.AccessToken)
                ])
                authProperties.IsPersistent <- true

                // CORREÇÃO: Passar authProperties como parâmetro para persistir o token no registo
                do! this.HttpContext.SignInAsync(AuthScheme, claimsPrincipal, authProperties)
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
                    this.ModelState.AddModelError("", "A API backend falhou ao processar o pedido de recuperação.")
                    return this.View() :> IActionResult
        }

    [<HttpPost>]
    member this.Logout() =
        task {
            // Limpa o cookie de autenticação local de forma segura
            do! this.HttpContext.SignOutAsync(AuthScheme)
            return this.RedirectToAction("Index", "Home") :> IActionResult
        }

    [<HttpGet>]
    member this.ResetPassword() =
        // Captura direta de parâmetros via Query String de forma segura
        let query = this.Request.Query
        let token = if query.ContainsKey("token") then query.["token"].ToString() else ""
        let email = if query.ContainsKey("email") then query.["email"].ToString() else ""

        if String.IsNullOrWhiteSpace(token) || String.IsNullOrWhiteSpace(email) then
            // Se o link de redefinição for inválido, redireciona para o Login
            this.RedirectToAction("Login") :> IActionResult
        else
            // Disponibiliza as variáveis na ViewData para renderização nas tags do formulário HTML
            this.ViewData["Token"] <- token
            this.ViewData["Email"] <- email
            this.View() :> IActionResult

    [<HttpPost>]
    member this.ResetPassword(form: Microsoft.AspNetCore.Http.IFormCollection) =
        task {
            // Captura explícita dos campos vindos do formulário HTML
            let email = form.["Email"].ToString()
            let token = form.["Token"].ToString()
            let newPassword = form.["NewPassword"].ToString()

            if String.IsNullOrWhiteSpace(email) || String.IsNullOrWhiteSpace(newPassword) then
                this.ModelState.AddModelError("", "Dados de recuperação inválidos.")
                return this.View() :> IActionResult
            else
                // Criação estruturada do DTO de redefinição
                let dto = ResetPasswordRequestDto(email, token, newPassword)
            
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