namespace ECHA.Web.Controllers

open Microsoft.AspNetCore.Mvc
open Microsoft.AspNetCore.Authorization
open System.Threading.Tasks
open ECHA.Core.DTOs
open Microsoft.AspNetCore.Http

[<Authorize(Roles = "Editor,Admin")>]
type QuizzesController (apiClient: ECHA.Web.Services.ApiClient) =
    inherit Controller()

    private member this.GetToken() =
        this.User.FindFirst("AccessToken")?.Value

    [<HttpGet>]
    member this.Index (nivel: string, tema: string) =
        task {
            let! quizzes = apiClient.ListQuizzesAsync(?nivel = (if string.IsNullOrEmpty nivel then None else Some nivel),
                                                       ?tema = (if string.IsNullOrEmpty tema then None else Some tema))
            return this.View(quizzes) :> IActionResult
        }

    [<HttpGet>]
    member this.Stats (id: int) =
        task {
            let token = this.GetToken()
            match token with
            | null -> return this.Unauthorized() :> IActionResult
            | t ->
                let! stats = apiClient.GetQuizStatsAsync(id, t)
                match stats with
                | Some s -> return this.View(s) :> IActionResult
                | None -> return this.NotFound() :> IActionResult
        }

    [<HttpGet>]
    member this.Pool (tema: string, nivel: System.Nullable<int>) =
        task {
            let token = this.GetToken()
            let n = if nivel.HasValue then Some nivel.Value else None
            let! perguntas = apiClient.GetQuestionPoolAsync(?tema = (if string.IsNullOrEmpty tema then None else Some tema),
                                                            ?nivel = n,
                                                            ?token = (if token = null then None else Some token))
            return this.View(perguntas) :> IActionResult
        }

    [<HttpGet>]
    member this.Create () =
        this.View()

    [<HttpPost>]
    member this.Create (request: CreateQuizDto) =
        task {
            let token = this.GetToken()
            match token with
            | null -> return this.Unauthorized() :> IActionResult
            | t ->
                let! success = apiClient.CreateQuizAsync(request, t)
                if success then
                    return this.RedirectToAction("Index") :> IActionResult
                else
                    this.ModelState.AddModelError("", "Falha ao criar quiz")
                    return this.View(request) :> IActionResult
        }

    [<HttpGet>]
    member this.Edit (id: int) =
        task {
            // Need a way to get full quiz for editing, currently GetById in API is internal for start
            // Let's assume the API's GetById returns everything for Admin/Editor
            // But the current QuizResponseDto is light.
            // For now, let's just use the index data if enough, or implement GetQuizDetails
            // I'll skip Edit implementation for now or implement GetQuizAsync in API
            return this.View() :> IActionResult
        }

    [<HttpPost>]
    member this.Delete (id: int) =
        task {
            let token = this.GetToken()
            match token with
            | null -> return this.Unauthorized() :> IActionResult
            | t ->
                let! success = apiClient.DeleteQuizAsync(id, t)
                if success then
                    return this.RedirectToAction("Index") :> IActionResult
                else
                    return this.BadRequest("Falha ao eliminar quiz") :> IActionResult
        }
