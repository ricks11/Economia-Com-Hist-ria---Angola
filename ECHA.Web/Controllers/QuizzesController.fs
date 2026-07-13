namespace ECHA.Web.Controllers

open Microsoft.AspNetCore.Mvc
open Microsoft.AspNetCore.Authorization
open System
open System.Threading.Tasks
open EconomiaComHistoria.Core.DTOs
open Microsoft.AspNetCore.Http
open ECHA.Web.Services
open Microsoft.AspNetCore.Authentication // Necessário para o GetTokenAsync

// Garante o esquema de autenticação correto por Cookie para evitar conflitos locais
[<Authorize(AuthenticationSchemes = "CookieAuthentication")>]
type QuizzesController (apiClient: ECHA.Web.Services.ApiClient) =
    inherit Controller()

    // Método auxiliar assíncrono para obter o token JWT de forma correta e limpa do Cookie
    member private this.GetTokenAsync() =
        task {
            let! token = this.HttpContext.GetTokenAsync("access_token")
            if String.IsNullOrEmpty(token) then return null
            else return token.Trim().Replace("\"", "")
        }

    [<HttpGet>]
    member this.Index (nivel: string, tema: string) =
        task {
            let! token = this.GetTokenAsync()
            match token with
            | null -> return this.RedirectToAction("Login", "Auth") :> IActionResult
            | t ->
                try
                    let! quizzes = apiClient.ListQuizzesAsync(
                                        t,
                                        ?nivel = (if String.IsNullOrEmpty nivel then None else Some nivel),
                                        ?tema = (if String.IsNullOrEmpty tema then None else Some tema))
                    return this.View(quizzes) :> IActionResult
                with
                | :? ApiClientException ->
                    return this.RedirectToAction("Login", "Auth") :> IActionResult
        }

    [<HttpGet>]
    [<Authorize(Roles = "Editor,Admin,SuperAdmin")>]
    member this.Stats (id: int) =
        task {
            let! token = this.GetTokenAsync()
            match token with
            | null -> return this.Unauthorized() :> IActionResult
            | t ->
                try
                    let! stats = apiClient.GetQuizStatsAsync(id, t)
                    match stats with
                    | Some s -> return this.View(s) :> IActionResult
                    | None -> return this.NotFound() :> IActionResult
                with
                | :? ApiClientException ->
                    return this.RedirectToAction("Login", "Auth") :> IActionResult
        }

    [<HttpGet>]
    [<Authorize(Roles = "Editor,Admin,SuperAdmin")>]
    member this.Pool (tema: string, nivel: System.Nullable<int>) =
        task {
            let! token = this.GetTokenAsync()
            match token with
            | null -> return this.Unauthorized() :> IActionResult
            | t ->
                let n = if nivel.HasValue then Some nivel.Value else None
                try
                    let! perguntas = apiClient.GetQuestionPoolAsync(
                                        ?tema = (if String.IsNullOrEmpty tema then None else Some tema),
                                        ?nivel = n,
                                        token = t)
                    return this.View(perguntas) :> IActionResult
                with
                | :? ApiClientException ->
                    return this.RedirectToAction("Login", "Auth") :> IActionResult
        }

    [<HttpGet>]
    [<Authorize(Roles = "Editor,Admin,SuperAdmin")>]
    member this.Create () =
        this.View()

    [<HttpPost>]
    [<Authorize(Roles = "Editor,Admin,SuperAdmin")>]
    member this.Create (request: CreateQuizDto) =
        task {
            let! token = this.GetTokenAsync()
            match token with
            | null -> return this.Unauthorized() :> IActionResult
            | t ->
                try
                    let! success = apiClient.CreateQuizAsync(request, t)
                    if success then
                        this.TempData["SuccessMessage"] <- "Quiz criado com sucesso!"
                        return this.RedirectToAction("Index") :> IActionResult
                    else
                        this.ModelState.AddModelError("", "Falha ao criar quiz")
                        return this.View(request) :> IActionResult
                with
                | :? ApiClientException ->
                    return this.RedirectToAction("Login", "Auth") :> IActionResult
        }

    [<HttpGet>]
    [<Authorize(Roles = "Editor,Admin,SuperAdmin")>]
    member this.Edit (id: int) =
        task {
            let! token = this.GetTokenAsync()
            match token with
            | null -> return this.Unauthorized() :> IActionResult
            | t ->
                try
                    let! quiz = apiClient.GetQuizDetalheAsync(id, t)
                    match quiz with
                    | Some q -> return this.View(q) :> IActionResult
                    | None -> return this.NotFound() :> IActionResult
                with
                | :? ApiClientException ->
                    return this.RedirectToAction("Login", "Auth") :> IActionResult
        }

    [<HttpPost>]
    [<Authorize(Roles = "Editor,Admin,SuperAdmin")>]
    member this.Edit (id: int, request: UpdateQuizDto) =
        task {
            let! token = this.GetTokenAsync()
            match token with
            | null -> return this.Unauthorized() :> IActionResult
            | t ->
                try
                    let! success = apiClient.UpdateQuizAsync(id, request, t)
                    if success then
                        this.TempData["SuccessMessage"] <- "Quiz atualizado com sucesso!"
                        return this.RedirectToAction("Index") :> IActionResult
                    else
                        this.TempData["ErrorMessage"] <- "Falha ao atualizar quiz"
                        let! quiz = apiClient.GetQuizDetalheAsync(id, t)
                        match quiz with
                        | Some q -> return this.View(q) :> IActionResult
                        | None -> return this.RedirectToAction("Index") :> IActionResult
                with
                | :? ApiClientException ->
                    return this.RedirectToAction("Login", "Auth") :> IActionResult
        }

    [<HttpPost>]
    [<Authorize(Roles = "Editor,Admin,SuperAdmin")>]
    member this.Delete (id: int) =
        task {
            let! token = this.GetTokenAsync()
            match token with
            | null -> return this.Unauthorized() :> IActionResult
            | t ->
                try
                    let! success = apiClient.DeleteQuizAsync(id, t)
                    if success then
                        this.TempData["SuccessMessage"] <- "Quiz excluído com sucesso!"
                        return this.RedirectToAction("Index") :> IActionResult
                    else
                        return this.BadRequest("Falha ao eliminar quiz") :> IActionResult
                with
                | :? ApiClientException ->
                    return this.RedirectToAction("Login", "Auth") :> IActionResult
        }

    [<HttpGet>]
    member this.Jogar (id: int) =
        task {
            let! token = this.GetTokenAsync()
            match token with
            | null -> return this.RedirectToAction("Login", "Auth") :> IActionResult
            | t ->
                try
                    // 1. Inicia o quiz na API (obtém as perguntas e o ID da tentativa)
                    let! startData = apiClient.StartQuizAsync(id, t)
                    match startData with
                    | Some sd ->
                        // 2. Procura os detalhes básicos do quiz para passar as informações de cabeçalho
                        let! quizDetails = apiClient.GetQuizDetalheAsync(id, t)
                        match quizDetails with
                        | Some qd ->
                            let qResponse = QuizResponseDto(qd.Id, qd.Titulo, qd.Tema, qd.Nivel, qd.TotalPerguntas, qd.TempoLimiteSeg, qd.Ativo)
                            this.ViewData["Quiz"] <- qResponse
                        | None -> ()
                        
                        return this.View(sd) :> IActionResult
                    | None -> return this.NotFound() :> IActionResult
                with
                | :? ApiClientException ->
                    return this.RedirectToAction("Login", "Auth") :> IActionResult
        }

    [<HttpPost>]
    member this.Submeter (dto: SubmitTentativaDto) =
        task {
            let! token = this.GetTokenAsync()
            match token with
            | null -> return this.Unauthorized() :> IActionResult
            | t ->
                try
                    let! resultado = apiClient.SubmitQuizAsync(dto, t)
                    match resultado with
                    | Some res -> 
                        // Retorna a View "Resultado" que exibe a pontuação final obtida
                        return this.View("Resultado", res) :> IActionResult
                    | None -> 
                        return this.BadRequest("Erro ao processar o envio do quiz.") :> IActionResult
                with
                | :? ApiClientException ->
                    return this.RedirectToAction("Login", "Auth") :> IActionResult
        }