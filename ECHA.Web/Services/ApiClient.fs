namespace ECHA.Web.Services

open System
open System.Net.Http
open System.Net.Http.Json
open System.Threading.Tasks
open System.Text.Json
open EconomiaComHistoria.Core.DTOs
open EconomiaComHistoria.Core.Enums

open System.Text.Json.Serialization
open System.Net.Http.Headers

module private JsonOpts =
    let options =
        let opts = JsonSerializerOptions(PropertyNamingPolicy = JsonNamingPolicy.CamelCase)
        opts.Converters.Add(JsonStringEnumConverter())
        opts

// Definição da Exceção com suporte a construtor de parâmetro único
type ApiClientException(statusCode: System.Net.HttpStatusCode, message: string) =
    inherit Exception(message)
    member this.StatusCode = statusCode
    // Construtor secundário para evitar erros quando passamos apenas a string do erro
    new(message: string) = ApiClientException(System.Net.HttpStatusCode.BadRequest, message)

type ApiClient(httpClient: HttpClient) =
    member this.LoginAsync(request: LoginRequestDto) : Task<AuthResponseDto option> =
        task {
            let! response = httpClient.PostAsJsonAsync("/api/auth/login", request)
            if response.IsSuccessStatusCode then
                let! authResponse = response.Content.ReadFromJsonAsync<AuthResponseDto>()
                return Some authResponse
            else
                return None
        }

    member this.RegisterAsync(request: RegisterRequestDto) : Task<AuthResponseDto option> =
        task {
            let! response = httpClient.PostAsJsonAsync("/api/auth/register", request)
            if response.IsSuccessStatusCode then
                let! authResponse = response.Content.ReadFromJsonAsync<AuthResponseDto>()
                return Some authResponse
            else
                return None
        }

    member this.ForgotPasswordAsync(email: string) : Task<bool> =
        task {
            let requestBody = dict [ "Email", email ]
            let! response = httpClient.PostAsJsonAsync("/api/auth/forgot-password", requestBody)
            return response.IsSuccessStatusCode
        }

    member this.ResetPasswordAsync(request: EconomiaComHistoria.Core.DTOs.ResetPasswordRequestDto) : Task<bool> =
        task {
            let! response = httpClient.PostAsJsonAsync("/api/auth/reset-password", request)
            return response.IsSuccessStatusCode
        }

    // Conteúdo Methods

    member this.ListConteudosAsync(?tema, ?nivel, ?regiao, ?tipo, ?pagina, ?tamanho, ?estado, ?jindungo) : Task<EconomiaComHistoria.Core.Helpers.PagedResult<ConteudoResponseDto>> =
        task {
            let mutable url = "/api/conteudos?"
            tema |> Option.iter (fun v -> url <- url + "tema=" + v + "&")
            nivel |> Option.iter (fun v -> url <- url + "nivel=" + v + "&")
            regiao |> Option.iter (fun v -> url <- url + "regiao=" + v + "&")
            tipo |> Option.iter (fun v -> url <- url + "tipo=" + v + "&")
            pagina |> Option.iter (fun v -> url <- url + "pagina=" + (string v) + "&")
            tamanho |> Option.iter (fun v -> url <- url + "tamanho=" + (string v) + "&")
            estado |> Option.iter (fun v -> url <- url + "estado=" + v + "&")
            jindungo |> Option.iter (fun v -> url <- url + "jindungo=" + (string v) + "&")

            let! response = httpClient.GetAsync(url)
            if response.IsSuccessStatusCode then
                // CORREÇÃO CRÍTICA: Usa os JsonOpts.options globais que contêm o JsonStringEnumConverter()
                // E mapeia diretamente para o wrapper PagedResult que a tua API envia
                let! pagedResult = response.Content.ReadFromJsonAsync<EconomiaComHistoria.Core.Helpers.PagedResult<ConteudoResponseDto>>(JsonOpts.options)
                return pagedResult
            else if (int response.StatusCode = 401) then
                return raise (ApiClientException(response.StatusCode, "Unauthorized"))
            else
                return EconomiaComHistoria.Core.Helpers.PagedResult<ConteudoResponseDto>() // Retorna uma instância de paginação vazia
        }

    member this.GetConteudoAsync(id: int, ?token: string, ?userId: int) : Task<ConteudoResponseDto option> =
        task {
            let url = 
                match userId with
                | Some uid -> $"/api/conteudos/{id}?userIdContext={uid}"
                | None -> $"/api/conteudos/{id}"

            // Se foi passado token, coloca-o no cabeçalho da requisição
            token |> Option.iter (fun t ->
                httpClient.DefaultRequestHeaders.Authorization <-
                    System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", t))

            // ✅ CORREÇÃO: Usa a variável 'url' construída acima!
            let! response = httpClient.GetAsync(url)
        
            // Limpa o cabeçalho depois para não afetar outras chamadas
            httpClient.DefaultRequestHeaders.Authorization <- null

            if response.IsSuccessStatusCode then
                let! conteudo = response.Content.ReadFromJsonAsync<ConteudoResponseDto>(JsonOpts.options)
                return Some conteudo
            else if (int response.StatusCode = 401) then
                return raise (ApiClientException(response.StatusCode, "Unauthorized"))
            else
                return None
        }

    member this.IncrementarVisitasAsync(id: int) : Task<bool> =
        task {
            let! response = httpClient.PostAsync($"/api/conteudos/{id}/incrementar-visita", null)
            return response.IsSuccessStatusCode
        }

    member this.CreateConteudoAsync(request: CreateConteudoDto, token: string) : Task<ConteudoResponseDto option> =
        task {
            httpClient.DefaultRequestHeaders.Authorization <- System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token)
        
            // CORREÇÃO: Passar JsonOpts.options como terceiro parâmetro para garantir camelCase e Enums em String
            let! response = httpClient.PostAsJsonAsync("/api/conteudos", request, JsonOpts.options)
        
            if response.IsSuccessStatusCode then
                let! conteudo = response.Content.ReadFromJsonAsync<ConteudoResponseDto>(JsonOpts.options)
                return Some conteudo
            else
                let! errorContent = response.Content.ReadAsStringAsync()
                let errorMessage = 
                    if String.IsNullOrWhiteSpace(errorContent) then response.ReasonPhrase
                    else errorContent
                return raise (ApiClientException(response.StatusCode, errorMessage))
        }

    member this.UpdateConteudoAsync(id: int, request: UpdateConteudoDto, token: string) : Task<ConteudoResponseDto option> =
        task {
            httpClient.DefaultRequestHeaders.Authorization <- System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token)
        
            // CORREÇÃO: Passar JsonOpts.options também no PUT
            let! response = httpClient.PutAsJsonAsync($"/api/conteudos/{id}", request, JsonOpts.options)
        
            if response.IsSuccessStatusCode then
                let! conteudo = response.Content.ReadFromJsonAsync<ConteudoResponseDto>(JsonOpts.options)
                return Some conteudo
            else if (int response.StatusCode = 401) then
                let! errorContent = response.Content.ReadAsStringAsync()
                return raise (ApiClientException(response.StatusCode, errorContent))
            else
                return None
        }

    member this.DeleteConteudoAsync(id: int, token: string) : Task<bool> =
        task {
            httpClient.DefaultRequestHeaders.Authorization <- System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token)
            let! response = httpClient.DeleteAsync($"/api/conteudos/{id}")
            if response.IsSuccessStatusCode then
                return true
            else if (int response.StatusCode = 401) then
                return raise (ApiClientException(response.StatusCode, "Unauthorized"))
            else
                return false
        }

    member this.UploadImagemCapaAsync(id: int, stream: System.IO.Stream, fileName: string, token: string) : Task<ConteudoResponseDto option> =
        task {
            httpClient.DefaultRequestHeaders.Authorization <- System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token)
            use content = new MultipartFormDataContent()
            use fileContent = new StreamContent(stream)
            content.Add(fileContent, "imagem", fileName)
            let! response = httpClient.PostAsync($"/api/conteudos/{id}/imagem", content)
            if response.IsSuccessStatusCode then
                let! conteudo = response.Content.ReadFromJsonAsync<ConteudoResponseDto>(JsonOpts.options)  // <- adicionar options
                return Some conteudo
            else if (int response.StatusCode = 401) then
                return raise (ApiClientException(response.StatusCode, "Unauthorized"))
            else
                return None
        }

    member this.ToggleFavoritoAsync(id: int, token: string) : Task<bool> =
        task {
            httpClient.DefaultRequestHeaders.Authorization <- 
                System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token)
        
            let! response = httpClient.PostAsync($"/api/conteudos/{id}/favorito", null)
        
            if response.IsSuccessStatusCode then
                let! result = response.Content.ReadFromJsonAsync<ToggleFavoritoResponseDto>(JsonOpts.options)
                // 👇 Alterado para "Adicionado" com letra maiúscula para bater com o C#
                return result.Adicionado 
            else if (int response.StatusCode = 401) then
                return raise (ApiClientException(response.StatusCode, "Sessão expirada ou não autorizado."))
            else
                return false
        }

    member this.ListFavoritosAsync(token: string, ?pagina: int, ?tamanho: int) : Task<EconomiaComHistoria.Core.Helpers.PagedResult<ConteudoResponseDto>> =
        task {
            httpClient.DefaultRequestHeaders.Authorization <- System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token)
            let mutable url = "/api/perfil/favoritos?"
            pagina |> Option.iter (fun v -> url <- url + "pagina=" + string v + "&")
            tamanho |> Option.iter (fun v -> url <- url + "tamanho=" + string v + "&")
            let! response = httpClient.GetAsync(url)
            if response.IsSuccessStatusCode then
                let! result = response.Content.ReadFromJsonAsync<EconomiaComHistoria.Core.Helpers.PagedResult<ConteudoResponseDto>>(JsonOpts.options)
                return result
            else if (int response.StatusCode = 401) then
                return raise (ApiClientException(response.StatusCode, "Unauthorized"))
            else
                return EconomiaComHistoria.Core.Helpers.PagedResult<ConteudoResponseDto>()
        }

    // Quiz Methods
    member this.GetQuizDetalheAsync(id: int, token: string) : Task<QuizDetalheDto option> =
        task {
            httpClient.DefaultRequestHeaders.Authorization <- System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token)
            let! response = httpClient.GetAsync($"/api/quizzes/{id}/detalhe")
            if response.IsSuccessStatusCode then
                let! result = response.Content.ReadFromJsonAsync<QuizDetalheDto>(JsonOpts.options)
                return Some result
            else if (int response.StatusCode = 401) then
                return raise (ApiClientException(response.StatusCode, "Unauthorized"))
            else
                return None
        }

    member this.ListQuizzesAsync(token: string, ?nivel: string, ?tema: string) : Task<QuizResponseDto list> =
        task {
            httpClient.DefaultRequestHeaders.Authorization <- System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token)
            let mutable url = "/api/quizzes?"
            nivel |> Option.iter (fun v -> url <- url + "nivel=" + v + "&")
            tema |> Option.iter (fun v -> url <- url + "tema=" + v + "&")

            let! response = httpClient.GetAsync(url)
            if response.IsSuccessStatusCode then
                let! result = response.Content.ReadFromJsonAsync<QuizResponseDto list>(JsonOpts.options)
                return result
            else if (int response.StatusCode = 401) then
                return raise (ApiClientException(response.StatusCode, "Unauthorized"))
            else
                return []
        }

    member this.GetQuizStatsAsync(id: int, token: string) : Task<QuizStatsDto option> =
        task {
            httpClient.DefaultRequestHeaders.Authorization <- System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token)
            let! response = httpClient.GetAsync($"/api/quizzes/{id}/stats")
            if response.IsSuccessStatusCode then
                let! result = response.Content.ReadFromJsonAsync<QuizStatsDto>(JsonOpts.options)
                return Some result
            else if (int response.StatusCode = 401) then
                return raise (ApiClientException(response.StatusCode, "Unauthorized"))
            else
                return None
        }

    member this.GetQuestionPoolAsync(?tema, ?nivel, ?token) : Task<PerguntaDetalheDto list> =
        task {
            // Injeta o token Bearer no cabeçalho se ele existir
            token |> Option.iter (fun t -> httpClient.DefaultRequestHeaders.Authorization <- System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", t))
        
            let mutable url = "/api/quizzes/pool?"
            tema |> Option.iter (fun v -> url <- url + "tema=" + v + "&")
            nivel |> Option.iter (fun v -> url <- url + "nivel=" + (string v) + "&")

            let! response = httpClient.GetAsync(url)
            if response.IsSuccessStatusCode then
                // CORREÇÃO: LER COMO PerguntaDetalheDto list PARA MANTER OS CAMPOS IsCorrecta E Explicacao
                let! result = response.Content.ReadFromJsonAsync<PerguntaDetalheDto list>()
                return result
            else if (int response.StatusCode = 401) then
                return raise (ApiClientException(response.StatusCode, "Unauthorized"))
            else
                return []
        }

    member this.CreateQuizAsync(request: CreateQuizDto, token: string) : Task<bool> =
        task {
            httpClient.DefaultRequestHeaders.Authorization <- System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token)
            let! response = httpClient.PostAsJsonAsync("/api/quizzes", request)
            if response.IsSuccessStatusCode then
                return true
            else if (int response.StatusCode = 401) then
                return raise (ApiClientException(response.StatusCode, "Unauthorized"))
            else
                return false
        }

    member this.UpdateQuizAsync(id: int, request: UpdateQuizDto, token: string) : Task<bool> =
        task {
            httpClient.DefaultRequestHeaders.Authorization <- System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token)
            let! response = httpClient.PutAsJsonAsync($"/api/quizzes/{id}", request)
            if response.IsSuccessStatusCode then
                return true
            else if (int response.StatusCode = 401) then
                return raise (ApiClientException(response.StatusCode, "Unauthorized"))
            else
                return false
        }

    member this.DeleteQuizAsync(id: int, token: string) : Task<bool> =
        task {
            httpClient.DefaultRequestHeaders.Authorization <- System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token)
            let! response = httpClient.DeleteAsync($"/api/quizzes/{id}")
            if response.IsSuccessStatusCode then
                return true
            else if (int response.StatusCode = 401) then
                return raise (ApiClientException(response.StatusCode, "Unauthorized"))
            else
                return false
        }

    member this.StartQuizAsync(id: int, token: string) : Task<QuizStartResponseDto option> =
        task {
            httpClient.DefaultRequestHeaders.Authorization <- System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token)
            let! response = httpClient.GetAsync($"/api/quizzes/{id}/start")
            if response.IsSuccessStatusCode then
                let! result = response.Content.ReadFromJsonAsync<QuizStartResponseDto>()
                return Some result
            else if (int response.StatusCode = 401) then
                return raise (ApiClientException(response.StatusCode, "Unauthorized"))
            else
                return None
        }

    member this.SubmitQuizAsync(request: SubmitTentativaDto, token: string) : Task<QuizSubmissionResponseDto option> =
        task {
            httpClient.DefaultRequestHeaders.Authorization <- System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token)
            let! response = httpClient.PostAsJsonAsync("/api/quizzes/tentativa", request)
            if response.IsSuccessStatusCode then
                let! result = response.Content.ReadFromJsonAsync<QuizSubmissionResponseDto>()
                return Some result
            else if (int response.StatusCode = 401) then
                return raise (ApiClientException(response.StatusCode, "Unauthorized"))
            else
                return None
        }

    // Moderation Methods
    member this.GetPendentesAsync(token: string) : Task<ModeracaoPendentesResponse option> =
        task {
            httpClient.DefaultRequestHeaders.Authorization <- System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token)
            let! response = httpClient.GetAsync("/api/moderacao/pendentes")
            if response.IsSuccessStatusCode then
                let! result = response.Content.ReadFromJsonAsync<ModeracaoPendentesResponse>()
                return Some result
            else if (int response.StatusCode = 401) then
                return raise (ApiClientException(response.StatusCode, "Unauthorized"))
            else
                return None
        }

    member this.GetDenunciasAsync(token: string) : Task<DenunciaSummaryDto list> =
        task {
            httpClient.DefaultRequestHeaders.Authorization <- System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token)
            let! response = httpClient.GetAsync("/api/moderacao/denuncias")
            if response.IsSuccessStatusCode then
                let! result = response.Content.ReadFromJsonAsync<DenunciaSummaryDto list>()
                return result
            else if (int response.StatusCode = 401) then
                return raise (ApiClientException(response.StatusCode, "Unauthorized"))
            else
                return []
        }

    member this.ListUtilizadoresAsync(token: string) : Task<UtilizadorModeracaoDto list> =
        task {
            httpClient.DefaultRequestHeaders.Authorization <- System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token)
            let! response = httpClient.GetAsync("/api/moderacao/utilizadores")
            if response.IsSuccessStatusCode then
                let! result = response.Content.ReadFromJsonAsync<UtilizadorModeracaoDto list>()
                return result
            else if (int response.StatusCode = 401) then
                return raise (ApiClientException(response.StatusCode, "Unauthorized"))
            else
                return []
        }

    member this.AprovarTopicoAsync(id: int, token: string) : Task<bool> =
        task {
            httpClient.DefaultRequestHeaders.Authorization <- System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token)
            let! response = httpClient.PutAsync($"/api/moderacao/topicos/{id}/aprovar", null)
            if response.IsSuccessStatusCode then
                return true
            else if (int response.StatusCode = 401) then
                return raise (ApiClientException(response.StatusCode, "Unauthorized"))
            else
                return false
        }

    member this.RejeitarTopicoAsync(id: int, request: RejeitarTopicoDto, token: string) : Task<bool> =
        task {
            httpClient.DefaultRequestHeaders.Authorization <- System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token)
            let! response = httpClient.PutAsJsonAsync($"/api/moderacao/topicos/{id}/rejeitar", request)
            if response.IsSuccessStatusCode then
                return true
            else if (int response.StatusCode = 401) then
                return raise (ApiClientException(response.StatusCode, "Unauthorized"))
            else
                return false
        }

    member this.AprovarRespostaAsync(id: int, token: string) : Task<bool> =
        task {
            httpClient.DefaultRequestHeaders.Authorization <- System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token)
            let! response = httpClient.PutAsync($"/api/moderacao/respostas/{id}/aprovar", null)
            if response.IsSuccessStatusCode then
                return true
            else if (int response.StatusCode = 401) then
                return raise (ApiClientException(response.StatusCode, "Unauthorized"))
            else
                return false
        }

    member this.RejeitarRespostaAsync(id: int, request: RejeitarTopicoDto, token: string) : Task<bool> =
        task {
            httpClient.DefaultRequestHeaders.Authorization <- System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token)
            let! response = httpClient.PutAsJsonAsync($"/api/moderacao/respostas/{id}/rejeitar", request)
            if response.IsSuccessStatusCode then
                return true
            else if (int response.StatusCode = 401) then
                return raise (ApiClientException(response.StatusCode, "Unauthorized"))
            else
                return false
        }

    member this.SuspenderUtilizadorAsync(id: int, request: SuspenderUtilizadorDto, token: string) : Task<bool> =
        task {
            httpClient.DefaultRequestHeaders.Authorization <- System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token)
            let! response = httpClient.PutAsJsonAsync($"/api/moderacao/utilizadores/{id}/suspender", request)
            if response.IsSuccessStatusCode then
                return true
            else if (int response.StatusCode = 401) then
                return raise (ApiClientException(response.StatusCode, "Unauthorized"))
            else
                return false
        }

    member this.ReativarUtilizadorAsync(id: int, token: string) : Task<bool> =
        task {
            httpClient.DefaultRequestHeaders.Authorization <- System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token)
            let! response = httpClient.PutAsync($"/api/moderacao/utilizadores/{id}/reativar", null)
            if response.IsSuccessStatusCode then
                return true
            else if (int response.StatusCode = 401) then
                return raise (ApiClientException(response.StatusCode, "Unauthorized"))
            else
                return false
        }

    // Gamification & Study Plan Methods
    member this.GetProgressoAsync(token: string) : Task<ProgressoUtilizadorDto option> =
        task {
            use request = new HttpRequestMessage(HttpMethod.Get, "/api/perfil/progresso")
            request.Headers.Authorization <- System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token)
        
            let! response = httpClient.SendAsync(request)
            if response.IsSuccessStatusCode then
                let! result = response.Content.ReadFromJsonAsync<ProgressoUtilizadorDto>(JsonOpts.options)
                return Some result
            else
                return None
        }

    member this.GerarPlanoEstudoAsync(token: string) : Task<bool> =
        task {
            httpClient.DefaultRequestHeaders.Authorization <- System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token)
            let! response = httpClient.PostAsync("/api/plano-estudo/gerar", null)
            if response.IsSuccessStatusCode then
                return true
            else if (int response.StatusCode = 401) then
                return raise (ApiClientException(response.StatusCode, "Unauthorized"))
            else
                return false
        }

    member this.GetBadgesAsync(token: string) : Task<BadgeAdminDto list> =
        task {
            httpClient.DefaultRequestHeaders.Authorization <- System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token)
            let! response = httpClient.GetAsync("/api/moderacao/badges")
            if response.IsSuccessStatusCode then
                let! result = response.Content.ReadFromJsonAsync<BadgeAdminDto list>(JsonOpts.options)
                return result
            else if (int response.StatusCode = 401) then
                return raise (ApiClientException(response.StatusCode, "Unauthorized"))
            else
                return []
        }

    member this.GetMetricasEngajamentoAsync(token: string) : Task<JsonElement option> =
        task {
            httpClient.DefaultRequestHeaders.Authorization <- System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token)
            let! response = httpClient.GetAsync("/api/moderacao/metricas-engajamento")
            if response.IsSuccessStatusCode then
                let! result = response.Content.ReadFromJsonAsync<JsonElement>()
                return Some result
            else if (int response.StatusCode = 401) then
                return raise (ApiClientException(response.StatusCode, "Unauthorized"))
            else
                return None
        }

    // Institutional Methods
    member this.ListEscolasAsync(token: string) : Task<EscolaResponseDto list> =
        task {
            httpClient.DefaultRequestHeaders.Authorization <- System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token)
            let! response = httpClient.GetAsync("/api/escolas")
            if response.IsSuccessStatusCode then
                let! result = response.Content.ReadFromJsonAsync<EscolaResponseDto list>()
                return result
            else if (int response.StatusCode = 401) then
                return raise (ApiClientException(response.StatusCode, "Unauthorized"))
            else
                return []
        }

    member this.CreateEscolaAsync(request: CreateEscolaDto, token: string) : Task<bool> =
        task {
            httpClient.DefaultRequestHeaders.Authorization <- System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token)
            let! response = httpClient.PostAsJsonAsync("/api/escolas", request)
            if response.IsSuccessStatusCode then
                return true
            else if (int response.StatusCode = 401) then
                return raise (ApiClientException(response.StatusCode, "Unauthorized"))
            else
                return false
        }

    member this.RevogarCodigoConviteAsync(escolaId: int, token: string) : Task<bool> =
        task {
            httpClient.DefaultRequestHeaders.Authorization <- System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token)
            let! response = httpClient.DeleteAsync($"/api/escolas/{escolaId}/convite")
            httpClient.DefaultRequestHeaders.Authorization <- null
            return response.IsSuccessStatusCode
        }

    member this.GerarCodigoConviteAsync(escolaId: int, token: string) : Task<InviteCodeResponseDto option> =
        task {
            httpClient.DefaultRequestHeaders.Authorization <- System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token)
            let! response = httpClient.PostAsync($"/api/escolas/{escolaId}/convite", null)
            if response.IsSuccessStatusCode then
                let! result = response.Content.ReadFromJsonAsync<InviteCodeResponseDto>()
                return Some result
            else if (int response.StatusCode = 401) then
                return raise (ApiClientException(response.StatusCode, "Unauthorized"))
            else
                return None
        }

    member this.ListTurmasAsync(token: string, ?escolaId: int) : Task<TurmaResponseDto list> =
        task {
            httpClient.DefaultRequestHeaders.Authorization <- System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token)
            let mutable url = "/api/turmas"
            escolaId |> Option.iter (fun id -> url <- url + $"?escolaId={id}")
            let! response = httpClient.GetAsync(url)
            if response.IsSuccessStatusCode then
                let! result = response.Content.ReadFromJsonAsync<TurmaResponseDto list>()
                return result
            else if (int response.StatusCode = 401) then
                return raise (ApiClientException(response.StatusCode, "Unauthorized"))
            else
                return []
        }

    member this.GetTurmaDetalheAsync(id: int, token: string) : Task<TurmaDetalheDto option> =
        task {
            httpClient.DefaultRequestHeaders.Authorization <- System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token)
            let! response = httpClient.GetAsync($"/api/turmas/{id}")
            if response.IsSuccessStatusCode then
                let! result = response.Content.ReadFromJsonAsync<TurmaDetalheDto>()
                return Some result
            else if (int response.StatusCode = 401) then
                return raise (ApiClientException(response.StatusCode, "Unauthorized"))
            else
                return None
        }

    member this.CreateTurmaAsync(request: CreateTurmaDto, token: string) : Task<bool> =
        task {
            httpClient.DefaultRequestHeaders.Authorization <- System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token)
            use! response = httpClient.PostAsJsonAsync("/api/turmas", request)
            httpClient.DefaultRequestHeaders.Authorization <- null

            if response.IsSuccessStatusCode then
                return true
            else if (int response.StatusCode = 400) then
                let! errorContent = response.Content.ReadAsStringAsync()
                // Tentar extrair a mensagem da resposta JSON
                try
                    use doc = JsonDocument.Parse(errorContent)
                    let message = doc.RootElement.GetProperty("message").GetString()
                    return raise (ApiClientException(response.StatusCode, message))
                with
                | _ -> return raise (ApiClientException(response.StatusCode, errorContent))
            else
                return false
        }

    // Adicionar/remover aluno da turma
    member this.AdicionarAlunoTurmaAsync(turmaId: int, alunoId: int, token: string) : Task<bool> =
        task {
            httpClient.DefaultRequestHeaders.Authorization <- System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token)
            let! response = httpClient.PostAsJsonAsync($"/api/turmas/{turmaId}/alunos", alunoId)
            httpClient.DefaultRequestHeaders.Authorization <- null
            return response.IsSuccessStatusCode
        }

    member this.RemoverAlunoTurmaAsync(turmaId: int, alunoId: int, token: string) : Task<bool> =
        task {
            httpClient.DefaultRequestHeaders.Authorization <- System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token)
            let! response = httpClient.DeleteAsync($"/api/turmas/{turmaId}/alunos/{alunoId}")
            httpClient.DefaultRequestHeaders.Authorization <- null
            return response.IsSuccessStatusCode
        }

    member this.ListProfessoresAsync(token: string) : Task<UtilizadorModeracaoDto list> =
        task {
            let! todos = this.ListUtilizadoresAsync(token)
            return todos |> List.filter (fun u -> u.Tipo = "Professor")
        }

    member this.GetPerfilAsync(token: string) : Task<PerfilResponseDto option> =
        task {
            use request = new HttpRequestMessage(HttpMethod.Get, "/api/perfil")
            request.Headers.Authorization <- System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token)
        
            let! response = httpClient.SendAsync(request)
        
            if response.IsSuccessStatusCode then
                let! result = response.Content.ReadFromJsonAsync<PerfilResponseDto>(JsonOpts.options)
                return Some result
            else if (int response.StatusCode = 401) then
                return raise (ApiClientException(response.StatusCode, "Unauthorized"))
            else
                return None
        }

    member this.UpdatePerfilAsync(request: UpdatePerfilDto, token: string) : Task<bool> =
        task {
            httpClient.DefaultRequestHeaders.Authorization <- System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token)
            let! response = httpClient.PutAsJsonAsync("/api/perfil", request)
            if response.IsSuccessStatusCode then
                return true
            else if (int response.StatusCode = 401) then
                return raise (ApiClientException(response.StatusCode, "Unauthorized"))
            else
                return false
        }

    member this.GetAuditoriaAsync(token: string, ?utilizadorId: int, ?acao: string, ?inicio: DateTime, ?fim: DateTime) : Task<AuditoriaLogDto list> =
        task {
            httpClient.DefaultRequestHeaders.Authorization <- System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token)
            let mutable url = "/api/admin/auditoria?"
            utilizadorId |> Option.iter (fun v -> url <- url + "utilizadorId=" + string v + "&")
            acao |> Option.iter (fun v -> url <- url + "acao=" + v + "&")
            inicio |> Option.iter (fun (v: DateTime) -> url <- url + "inicio=" + v.ToString("o") + "&")
            fim |> Option.iter (fun (v: DateTime) -> url <- url + "fim=" + v.ToString("o") + "&")

            let! response = httpClient.GetAsync(url)
            if response.IsSuccessStatusCode then
                let! result = response.Content.ReadFromJsonAsync<AuditoriaLogDto list>()
                return result
            else if (int response.StatusCode = 401) then
                return raise (ApiClientException(response.StatusCode, "Unauthorized"))
            else
                return []
        }

    member this.AlterarRoleAsync(id: int, request: RoleChangeDto, token: string) : Task<bool> =
        task {
            httpClient.DefaultRequestHeaders.Authorization <- System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token)
            let! response = httpClient.PutAsJsonAsync($"/api/admin/utilizadores/{id}/role", request)
            if response.IsSuccessStatusCode then
                return true
            else if (int response.StatusCode = 401) then
                return raise (ApiClientException(response.StatusCode, "Unauthorized"))
            else
                return false
        }

    member this.GetEscolaAsync(id: int, token: string) : Task<EscolaResponseDto option> =
        task {
            httpClient.DefaultRequestHeaders.Authorization <- System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token)
            let! response = httpClient.GetAsync($"/api/escolas/{id}")
            if response.IsSuccessStatusCode then
                let! result = response.Content.ReadFromJsonAsync<EscolaResponseDto>()
                return Some result
            else if (int response.StatusCode = 401) then
                return raise (ApiClientException(response.StatusCode, "Unauthorized"))
            else
                return None
        }

    member this.UpdateEscolaAsync(id: int, request: CreateEscolaDto, token: string) : Task<EscolaResponseDto option> =
        task {
            httpClient.DefaultRequestHeaders.Authorization <- System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token)
            let! response = httpClient.PutAsJsonAsync($"/api/escolas/{id}", request)
            if response.IsSuccessStatusCode then
                let! result = response.Content.ReadFromJsonAsync<EscolaResponseDto>()
                return Some result
            else if (int response.StatusCode = 401) then
                return raise (ApiClientException(response.StatusCode, "Unauthorized"))
            else
                return None
        }

    member this.DeleteEscolaAsync(id: int, token: string) : Task<bool> =
        task {
            httpClient.DefaultRequestHeaders.Authorization <- System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token)
            let! response = httpClient.DeleteAsync($"/api/escolas/{id}")
            if response.IsSuccessStatusCode then
                return true
            else if (int response.StatusCode = 401) then
                return raise (ApiClientException(response.StatusCode, "Unauthorized"))
            else
                return false
        }

    member this.UpdateTurmaAsync(id: int, request: UpdateTurmaDto, token: string) : Task<TurmaResponseDto option> =
        task {
            httpClient.DefaultRequestHeaders.Authorization <- System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token)
            let! response = httpClient.PutAsJsonAsync($"/api/turmas/{id}", request)
            if response.IsSuccessStatusCode then
                let! result = response.Content.ReadFromJsonAsync<TurmaResponseDto>()
                return Some result
            else if (int response.StatusCode = 401) then
                return raise (ApiClientException(response.StatusCode, "Unauthorized"))
            else
                return None
        }

    member this.DeleteTurmaAsync(id: int, token: string) : Task<bool> =
        task {
            httpClient.DefaultRequestHeaders.Authorization <- System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token)
            let! response = httpClient.DeleteAsync($"/api/turmas/{id}")
            if response.IsSuccessStatusCode then
                return true
            else if (int response.StatusCode = 401) then
                return raise (ApiClientException(response.StatusCode, "Unauthorized"))
            else
                return false
        }

    // Forum Methods
    member this.ListCategoriasForumAsync() : Task<CategoriaForumDto list> =
        task {
            let! response = httpClient.GetAsync("/api/forum/categorias")
            if response.IsSuccessStatusCode then
                let! result = response.Content.ReadFromJsonAsync<CategoriaForumDto list>(JsonOpts.options)
                return result
            else
                return []
        }

    member this.ListTopicosAsync(?categoriaId: int, ?ordem: string, ?token: string, ?incluirArquivados: bool) : Task<TopicoForumDto list> =
        task {
            let mutable url = "/api/forum/topicos?"
            categoriaId |> Option.iter (fun v -> url <- url + "categoriaId=" + string v + "&")
            ordem |> Option.iter (fun v -> url <- url + "ordem=" + v + "&")
            incluirArquivados |> Option.iter (fun v -> url <- url + "incluirArquivados=" + (string v).ToLower() + "&")

            // Envia o token se fornecido
            token |> Option.iter (fun t ->
                httpClient.DefaultRequestHeaders.Authorization <-
                    System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", t))

            let! response = httpClient.GetAsync(url)

            // Limpa o cabeçalho para não afetar outras chamadas
            httpClient.DefaultRequestHeaders.Authorization <- null

            if response.IsSuccessStatusCode then
                let! result = response.Content.ReadFromJsonAsync<TopicoForumDto list>(JsonOpts.options)
                return result
            else if (int response.StatusCode = 401) then
                return raise (ApiClientException(response.StatusCode, "Unauthorized"))
            else
                return []
        }

    member this.DesarquivarTopicoAsync(id: int, token: string) : Task<bool> =
        task {
            httpClient.DefaultRequestHeaders.Authorization <- System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token)
            let! response = httpClient.PutAsync($"/api/forum/topicos/{id}/desarquivar", null)
            httpClient.DefaultRequestHeaders.Authorization <- null
            return response.IsSuccessStatusCode
        }

    member this.GetTopicoAsync(id: int, ?token: string) : Task<TopicoForumDetalheDto option> =
        task {
            // Envia token se fornecido
            token |> Option.iter (fun t ->
                httpClient.DefaultRequestHeaders.Authorization <-
                    System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", t))

            let! response = httpClient.GetAsync($"/api/forum/topicos/{id}")

            // Limpa o cabeçalho após a chamada
            httpClient.DefaultRequestHeaders.Authorization <- null

            if response.IsSuccessStatusCode then
                let! result = response.Content.ReadFromJsonAsync<TopicoForumDetalheDto>(JsonOpts.options)
                return Some result
            else if (int response.StatusCode = 401) then
                return raise (ApiClientException(response.StatusCode, "Unauthorized"))
            else
                return None
        }

    member this.CriarTopicoAsync(request: CriarTopicoForumDto, token: string) : Task<TopicoForumDto option> =
        task {
            httpClient.DefaultRequestHeaders.Authorization <- System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token)
            let! response = httpClient.PostAsJsonAsync("/api/forum/topicos", request)
            if response.IsSuccessStatusCode then
                let! result = response.Content.ReadFromJsonAsync<TopicoForumDto>(JsonOpts.options)
                return Some result
            else if (int response.StatusCode = 401) then
                return raise (ApiClientException(response.StatusCode, "Unauthorized"))
            else
                return None
        }

    member this.GetRankingAsync(tipo: string, periodo: string, token: string) : Task<RankingResponseDto option> =
        task {
            httpClient.DefaultRequestHeaders.Authorization <- System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token)
            let! response = httpClient.GetAsync($"/api/ranking?tipo={tipo}&periodo={periodo}")
            if response.IsSuccessStatusCode then
                let! result = response.Content.ReadFromJsonAsync<RankingResponseDto>(JsonOpts.options)
                return Some result
            else if (int response.StatusCode = 401) then
                return raise (ApiClientException(response.StatusCode, "Unauthorized"))
            else
                return None
        }

    member this.GetRankingByUrlAsync(url: string, token: string) : Task<RankingResponseDto option> =
        task {
            httpClient.DefaultRequestHeaders.Authorization <- System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token)
            let! response = httpClient.GetAsync(url)
            httpClient.DefaultRequestHeaders.Authorization <- null

            if response.IsSuccessStatusCode then
                let! result = response.Content.ReadFromJsonAsync<RankingResponseDto>(JsonOpts.options)
                return Some result
            else if (int response.StatusCode = 401) then
                return raise (ApiClientException(response.StatusCode, "Unauthorized"))
            else
                return None
        }

    member this.AdicionarRespostaAsync(topicoId: int, request: CriarRespostaForumDto, token: string) : Task<bool> =
        task {
            httpClient.DefaultRequestHeaders.Authorization <- System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token)
            let! response = httpClient.PostAsJsonAsync($"/api/forum/topicos/{topicoId}/respostas", request)
            if response.IsSuccessStatusCode then
                return true
            else if (int response.StatusCode = 401) then
                return raise (ApiClientException(response.StatusCode, "Unauthorized"))
            else
                return false
        }

    member this.ApagarTopicoAsync(id: int, token: string) : Task<bool> =
        task {
            httpClient.DefaultRequestHeaders.Authorization <- System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token)
            let! response = httpClient.DeleteAsync($"/api/forum/topicos/{id}")
            if response.IsSuccessStatusCode then
                return true
            else if (int response.StatusCode = 401) then
                return raise (ApiClientException(response.StatusCode, "Unauthorized"))
            else
                return false
        }

    member this.DenunciarAsync(request: CriarDenunciaDto, token: string) : Task<bool> =
        task {
            httpClient.DefaultRequestHeaders.Authorization <- System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token)
            let! response = httpClient.PostAsJsonAsync("/api/forum/denuncias", request)
            if response.IsSuccessStatusCode then
                return true
            else if (int response.StatusCode = 401) then
                return raise (ApiClientException(response.StatusCode, "Unauthorized"))
            else
                return false
        }

    member this.ToggleReacaoAsync(request: CriarReacaoDto, token: string) : Task<bool> =
        task {
            httpClient.DefaultRequestHeaders.Authorization <- System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token)
            let! response = httpClient.PostAsJsonAsync("/api/forum/reacoes", request)
            return response.IsSuccessStatusCode
        }

    member this.ObterRespostaAsync(id: int, ?token: string) : Task<RespostaForumDto option> =
        task {
            token |> Option.iter (fun t ->
                httpClient.DefaultRequestHeaders.Authorization <-
                    System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", t))

            let! response = httpClient.GetAsync($"/api/forum/respostas/{id}")
            httpClient.DefaultRequestHeaders.Authorization <- null

            if response.IsSuccessStatusCode then
                let! result = response.Content.ReadFromJsonAsync<RespostaForumDto>(JsonOpts.options)
                return Some result
            else if (int response.StatusCode = 401) then
                return raise (ApiClientException(response.StatusCode, "Unauthorized"))
            else
                return None
        }

    member this.ListAlunosAsync(token: string) : Task<UtilizadorModeracaoDto list> =
        task {
            let! todos = this.ListUtilizadoresAsync(token)
            return todos |> List.filter (fun u -> u.Tipo = "Aluno")
        }

    member this.ListarRelatoriosAsync(token: string, ?escolaId: int) : Task<RelatorioListaDto list> =
        task {
            let mutable url = "/api/relatorios"
            escolaId |> Option.iter (fun id -> url <- url + $"?escolaId={id}")
            httpClient.DefaultRequestHeaders.Authorization <- System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token)
            let! response = httpClient.GetAsync(url)
            httpClient.DefaultRequestHeaders.Authorization <- null
            if response.IsSuccessStatusCode then
                let! result = response.Content.ReadFromJsonAsync<RelatorioListaDto list>(JsonOpts.options)
                return result
            else
                return []
        }

    member this.SolicitarRelatorioAsync(request: SolicitarRelatorioDto, token: string) : Task<RelatorioStatusDto option> =
        task {
            httpClient.DefaultRequestHeaders.Authorization <- System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token)
            let! response = httpClient.PostAsJsonAsync("/api/relatorios/gerar", request, JsonOpts.options)
            httpClient.DefaultRequestHeaders.Authorization <- null
            if response.IsSuccessStatusCode then
                let! result = response.Content.ReadFromJsonAsync<RelatorioStatusDto>(JsonOpts.options)
                return Some result
            else
                return None
        }

    member this.GetRelatorioStatusAsync(id: int, token: string) : Task<RelatorioStatusDto option> =
        task {
            httpClient.DefaultRequestHeaders.Authorization <- System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token)
            let! response = httpClient.GetAsync($"/api/relatorios/{id}/status")
            httpClient.DefaultRequestHeaders.Authorization <- null
            if response.IsSuccessStatusCode then
                let! result = response.Content.ReadFromJsonAsync<RelatorioStatusDto>(JsonOpts.options)
                return Some result
            else
                return None
        }

    member this.DownloadRelatorioAsync (id: int, token: string) : Task<byte[] option> =
        task {
            // 1. Configura a requisição GET para o endpoint da API
            let url = sprintf "api/relatorios/%d/download" id
            use request = new HttpRequestMessage(HttpMethod.Get, url)
            
            // 2. Injeta o Token JWT que veio do Web Controller
            request.Headers.Authorization <- AuthenticationHeaderValue("Bearer", token)
            
            // 3. Envia o pedido à API
            let! response = httpClient.SendAsync(request)
            
            // 4. Se a API responder 200 OK, lê os bytes; caso contrário (404, 401), retorna None
            if response.IsSuccessStatusCode then
                let! bytes = response.Content.ReadAsByteArrayAsync()
                return Some bytes
            else
                return None
        }