namespace ECHA.Web.Controllers

open Microsoft.AspNetCore.Mvc
open Microsoft.AspNetCore.Authorization
open Microsoft.AspNetCore.Routing
open System
open System.Threading.Tasks
open EconomiaComHistoria.Core.DTOs
open Microsoft.AspNetCore.Http
open ECHA.Web.Services

[<Authorize(Roles = "Admin,SuperAdmin,Editor")>]
type TurmasController (apiClient: ECHA.Web.Services.ApiClient) =
    inherit Controller()

    member private this.GetToken() =
        let claim = this.User.FindFirst("AccessToken")
        if isNull claim then null else claim.Value

    [<HttpGet>]
    member this.Index (escolaId: System.Nullable<int>) =
        task {
            let token = this.GetToken()
            match token with
            | null -> return this.Unauthorized() :> IActionResult
            | t ->
                let eId = if escolaId.HasValue then Some escolaId.Value else None
                let! turmas = apiClient.ListTurmasAsync(t, ?escolaId = eId)
                return this.View(turmas) :> IActionResult
        }

    [<HttpGet>]
    member this.Details (id: int) =
        task {
            let token = this.GetToken()
            match token with
            | null -> return this.Unauthorized() :> IActionResult
            | t ->
                let! turmaOpt = apiClient.GetTurmaDetalheAsync(id, t)
                match turmaOpt with
                | Some turma ->
                    // 1. Procura todos os utilizadores com a role "Aluno"
                    let! todosAlunos = apiClient.ListAlunosAsync(t) // Troca pelo nome correto do teu método da API
                
                    // 2. Opcional: Filtrar para não mostrar alunos que JÁ estão nesta turma
                    let alunosInscritosIds = turma.Alunos |> Seq.map (fun a -> a.Id) |> Set.ofSeq
                    let alunosDisponiveis = 
                        todosAlunos 
                        |> Seq.filter (fun a -> not (alunosInscritosIds.Contains(a.Id)))

                    // 3. Envia para a View
                    this.ViewData.["AlunosDisponiveis"] <- alunosDisponiveis
                    return this.View(turma) :> IActionResult
                
                | None -> return this.NotFound() :> IActionResult
        }

    [<HttpGet>]
    member this.Create (escolaId: int) =
        task {
            let token = this.GetToken()
            match token with
            | null -> return this.Unauthorized() :> IActionResult
            | t ->
                // Correção: Adicionado bloco 'else' para manter a integridade dos tipos
                if escolaId <= 0 then
                    this.TempData["ErrorMessage"] <- "ID da escola inválido. Selecione uma escola primeiro."
                    return this.RedirectToAction("Index", "Escolas") :> IActionResult
                else
                    let! professores = apiClient.ListProfessoresAsync(t)
                    this.ViewData.["Professores"] <- professores
                    let dto = new CreateTurmaDto("", System.Nullable<int>(), escolaId, 0)
                    return this.View(dto) :> IActionResult
        }

    [<HttpPost>]
    member this.Create (request: CreateTurmaDto) =
        task {
            let token = this.GetToken()
            match token with
            | null -> return this.Unauthorized() :> IActionResult
            | t ->
                // Correção: Transformado em uma árvore condicional pura (if / elif / else)
                if request.ProfessorId <= 0 then
                    this.TempData["ErrorMessage"] <- "Selecione um professor responsável."
                    let! professores = apiClient.ListProfessoresAsync(t)
                    this.ViewData.["Professores"] <- professores
                    return this.View(request) :> IActionResult

                elif request.EscolaId <= 0 then
                    this.TempData["ErrorMessage"] <- "ID da escola inválido. Tente novamente a partir da lista de escolas."
                    return this.RedirectToAction("Index", "Escolas") :> IActionResult

                else
                    try
                        let! success = apiClient.CreateTurmaAsync(request, t)
                        if success then
                            this.TempData["SuccessMessage"] <- "Turma criada com sucesso!"
                            let routeValues = RouteValueDictionary([ ("escolaId", box request.EscolaId) ])
                            return this.RedirectToAction("Index", routeValues) :> IActionResult
                        else
                            this.TempData["ErrorMessage"] <- "Erro ao criar turma (resposta inesperada)."
                            let! professores = apiClient.ListProfessoresAsync(t)
                            this.ViewData.["Professores"] <- professores
                            return this.View(request) :> IActionResult
                    with
                    | :? ApiClientException as ex ->
                        this.TempData["ErrorMessage"] <- ex.Message
                        let! professores = apiClient.ListProfessoresAsync(t)
                        this.ViewData.["Professores"] <- professores
                        return this.View(request) :> IActionResult
                    | ex ->
                        this.TempData["ErrorMessage"] <- "Erro inesperado: " + ex.Message
                        let! professores = apiClient.ListProfessoresAsync(t)
                        this.ViewData.["Professores"] <- professores
                        return this.View(request) :> IActionResult
        }

    [<HttpGet>]
    member this.Edit (id: int) =
        task {
            let token = this.GetToken()
            match token with
            | null -> return this.Unauthorized() :> IActionResult
            | t ->
                let! turma = apiClient.GetTurmaDetalheAsync(id, t)
                match turma with
                | Some tDetails -> 
                    // Carrega os professores para popular a dropdown
                    let! professores = apiClient.ListProfessoresAsync(t)
                    this.ViewData.["Professores"] <- professores
                    // Guarda o EscolaId para o fluxo de navegação de retorno
                    this.ViewData.["EscolaId"] <- tDetails.EscolaId
                    
                    let updateDto = new UpdateTurmaDto(tDetails.Nome, tDetails.Ano, tDetails.ProfessorId)
                    return this.View(updateDto) :> IActionResult
                | None -> return this.NotFound() :> IActionResult
        }

    [<HttpPost>]
    member this.Edit (id: int, request: UpdateTurmaDto) =
        task {
            let token = this.GetToken()
            match token with
            | null -> return this.Unauthorized() :> IActionResult
            | t ->
                // Validação idêntica à do Create para o ProfessorId
                if not request.ProfessorId.HasValue || request.ProfessorId.Value <= 0 then
                    this.TempData["ErrorMessage"] <- "Selecione um professor responsável."
                    let! professores = apiClient.ListProfessoresAsync(t)
                    this.ViewData.["Professores"] <- professores
                    let! currentTurma = apiClient.GetTurmaDetalheAsync(id, t)
                    match currentTurma with | Some ct -> this.ViewData.["EscolaId"] <- ct.EscolaId | None -> ()
                    return this.View(request) :> IActionResult
                else
                    try
                        let! result = apiClient.UpdateTurmaAsync(id, request, t)
                        match result with
                        | Some r ->
                            this.TempData["SuccessMessage"] <- "Turma atualizada com sucesso!"
                            let routeValues = RouteValueDictionary([ ("escolaId", box r.EscolaId) ])
                            return this.RedirectToAction("Index", routeValues) :> IActionResult
                        | None ->
                            this.TempData["ErrorMessage"] <- "Erro ao atualizar turma."
                            let! professores = apiClient.ListProfessoresAsync(t)
                            this.ViewData.["Professores"] <- professores
                            let! currentTurma = apiClient.GetTurmaDetalheAsync(id, t)
                            match currentTurma with | Some ct -> this.ViewData.["EscolaId"] <- ct.EscolaId | None -> ()
                            return this.View(request) :> IActionResult
                    with
                    | :? ApiClientException as ex ->
                        this.TempData["ErrorMessage"] <- ex.Message
                        let! professores = apiClient.ListProfessoresAsync(t)
                        this.ViewData.["Professores"] <- professores
                        return this.View(request) :> IActionResult
                    | ex ->
                        this.TempData["ErrorMessage"] <- "Erro inesperado: " + ex.Message
                        let! professores = apiClient.ListProfessoresAsync(t)
                        this.ViewData.["Professores"] <- professores
                        return this.View(request) :> IActionResult
        }

    [<HttpPost>]
    member this.Delete (id: int, escolaId: System.Nullable<int>) =
        task {
            let token = this.GetToken()
            match token with
            | null -> return this.Unauthorized() :> IActionResult
            | t ->
                let! success = apiClient.DeleteTurmaAsync(id, t)
                if success then
                    this.TempData["SuccessMessage"] <- "Turma eliminada com sucesso."
                else
                    this.TempData["ErrorMessage"] <- "Erro ao eliminar turma."
                let routeValues = RouteValueDictionary([ ("escolaId", box escolaId) ])
                return this.RedirectToAction("Index", routeValues) :> IActionResult
        }

    [<HttpPost>]
    member this.AdicionarAluno (turmaId: int, alunoId: int) =
        task {
            let token = this.GetToken()
            match token with
            | null -> return this.Unauthorized() :> IActionResult
            | t ->
                let! success = apiClient.AdicionarAlunoTurmaAsync(turmaId, alunoId, t)
                if success then
                    this.TempData["SuccessMessage"] <- "Aluno adicionado à turma."
                else
                    this.TempData["ErrorMessage"] <- "Erro ao adicionar aluno."
                let routeValues = RouteValueDictionary([ ("id", box turmaId) ])
                return this.RedirectToAction("Details", routeValues) :> IActionResult
        }

    [<HttpPost>]
    member this.RemoverAluno (turmaId: int, alunoId: int) =
        task {
            let token = this.GetToken()
            match token with
            | null -> return this.Unauthorized() :> IActionResult
            | t ->
                let! success = apiClient.RemoverAlunoTurmaAsync(turmaId, alunoId, t)
                if success then
                    this.TempData["SuccessMessage"] <- "Aluno removido da turma."
                else
                    this.TempData["ErrorMessage"] <- "Erro ao remover aluno."
                let routeValues = RouteValueDictionary([ ("id", box turmaId) ])
                return this.RedirectToAction("Details", routeValues) :> IActionResult
        }