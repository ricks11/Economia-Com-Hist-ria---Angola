namespace ECHA.Web.Controllers

open Microsoft.AspNetCore.Mvc
open Microsoft.AspNetCore.Authorization
open System
open System.Threading.Tasks
open EconomiaComHistoria.Core.DTOs
open Microsoft.AspNetCore.Http
open ECHA.Web.Services

type QuizzesController (apiClient: ECHA.Web.Services.ApiClient) =
    inherit Controller()

    member private this.GetToken() =
        let claim = this.User.FindFirst("AccessToken")
        if isNull claim then null else claim.Value

    [<HttpGet>]
    [<AllowAnonymous>]
    member this.Index (nivel: string, tema: string) =
        task {
            try
                let! quizzes = apiClient.ListQuizzesAsync(?nivel = (if String.IsNullOrEmpty nivel then None else Some nivel),
                                                           ?tema = (if String.IsNullOrEmpty tema then None else Some tema))
                return this.View(quizzes) :> IActionResult
            with
            | :? ApiClientException ->
                return this.RedirectToAction("Login", "Auth") :> IActionResult
        }

    [<HttpGet>]
    [<Authorize(Roles = "Editor,Admin")>]
    member this.Stats (id: int) =
        task {
            let token = this.GetToken()
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
    [<Authorize>]
    member this.Pool (tema: string, nivel: System.Nullable<int>) =
        task {
            let token = this.GetToken()
            let n = if nivel.HasValue then Some nivel.Value else None
            let! perguntas = apiClient.GetQuestionPoolAsync(?tema = (if String.IsNullOrEmpty tema then None else Some tema),
                                                              ?nivel = n,
                                                              ?token = (if token = null then None else Some token))
            return this.View(perguntas) :> IActionResult
        }

    [<HttpGet>]
    [<Authorize(Roles = "Editor,Admin")>]
    member this.Create () =
        this.View()

    [<HttpPost>]
    [<Authorize(Roles = "Editor,Admin")>]
    member this.Create (request: CreateQuizDto) =
        task {
            let token = this.GetToken()
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
    [<Authorize(Roles = "Editor,Admin")>]
    member this.Edit (id: int) =
        task {
            let token = this.GetToken()
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
    [<Authorize(Roles = "Editor,Admin")>]
    member this.Edit (id: int, request: UpdateQuizDto) =
        task {
            let token = this.GetToken()
            match token with
            | null -> return this.Unauthorized() :> IActionResult
            | t ->
                try
                    let! success = apiClient.UpdateQuizAsync(id, request, t)
                    if success then
                        this.TempData["SuccessMessage"] <- "Quiz atualizado com sucesso!"
                        return this.RedirectToAction("Index") :> IActionResult
                    else
                        this.ModelState.AddModelError("", "Falha ao atualizar quiz")
                        return this.View(request) :> IActionResult
                with
                | :? ApiClientException ->
                    return this.RedirectToAction("Login", "Auth") :> IActionResult
        }

    [<HttpPost>]
    [<Authorize(Roles = "Editor,Admin")>]
    member this.Delete (id: int) =
        task {
            let token = this.GetToken()
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
    [<Authorize>]
    member this.Jogar (id: int) =
        task {
            let token = this.GetToken()
            match token with
            | null -> return this.RedirectToAction("Login", "Auth") :> IActionResult
            | t ->
                try
                    let! quizzes = apiClient.ListQuizzesAsync()
                    let quizInfo = quizzes |> List.tryFind (fun q -> q.Id = id)
                    let! session = apiClient.StartQuizAsync(id, t)
                    match session, quizInfo with
                    | Some s, Some q ->
                        this.ViewData.["Quiz"] <- q
                        return this.View(s) :> IActionResult
                    | _, _ ->
                        this.TempData["ErrorMessage"] <- "Não foi possível iniciar o quiz. Tente novamente mais tarde."
                        return this.RedirectToAction("Index") :> IActionResult
                with
                | :? ApiClientException ->
                    return this.RedirectToAction("Login", "Auth") :> IActionResult
        }

    [<HttpPost>]
    [<Authorize>]
    member this.Submeter (request: SubmitTentativaDto) =
        task {
            let token = this.GetToken()
            match token with
            | null -> return this.RedirectToAction("Login", "Auth") :> IActionResult
            | t ->
                try
                    let! result = apiClient.SubmitQuizAsync(request, t)
                    match result with
                    | Some r ->
                        this.TempData["QuizResult"] <- System.Text.Json.JsonSerializer.Serialize(r)
                        return this.RedirectToAction("Resultado") :> IActionResult
                    | None ->
                        this.TempData["ErrorMessage"] <- "Falha ao submeter o quiz."
                        return this.RedirectToAction("Index") :> IActionResult
                with
                | :? ApiClientException ->
                    return this.RedirectToAction("Login", "Auth") :> IActionResult
        }

    [<HttpGet>]
    [<Authorize>]
    member this.Resultado () =
        task {
            match this.TempData["QuizResult"] with
            | null ->
                return this.RedirectToAction("Index") :> IActionResult
            | :? string as json ->
                let result = System.Text.Json.JsonSerializer.Deserialize<QuizSubmissionResponseDto>(json)
                return this.View(result) :> IActionResult
            | _ ->
                return this.RedirectToAction("Index") :> IActionResult
        }