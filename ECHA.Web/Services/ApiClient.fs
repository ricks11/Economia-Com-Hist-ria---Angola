namespace ECHA.Web.Services

open System
open System.Net.Http
open System.Net.Http.Json
open System.Threading.Tasks
open System.Text.Json
open System.Text.Json.Serialization
open System.Collections.Generic
open EconomiaComHistoria.Core.DTOs

module private JsonOpts =
    let options =
        let opts = JsonSerializerOptions(PropertyNamingPolicy = JsonNamingPolicy.CamelCase)
        opts.Converters.Add(JsonStringEnumConverter())
        opts

type ApiClientException(statusCode: System.Net.HttpStatusCode, message: string) =
    inherit Exception(message)
    member this.StatusCode = statusCode
    new(message: string) = ApiClientException(System.Net.HttpStatusCode.BadRequest, message)

type ApiClient(httpClient: HttpClient) =

    member private this.SetAuth (token: string) =
        httpClient.DefaultRequestHeaders.Authorization <- System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token)

    member this.LoginAsync(request: LoginRequestDto) : Task<AuthResponseDto option> =
        task {
            let! response = httpClient.PostAsJsonAsync("/api/auth/login", request, JsonOpts.options)
            if response.IsSuccessStatusCode then
                let! authResponse = response.Content.ReadFromJsonAsync<AuthResponseDto>(JsonOpts.options)
                return Some authResponse
            else
                // Captura o erro do backend se for um bloqueio (400 ou 403)
                let! errorObj = response.Content.ReadFromJsonAsync<ErrorResponseDto>(JsonOpts.options)
                // Lançamos uma exceção customizada ou guardamos na HttpContext para o Controller ler
                raise (ApiClientException(response.StatusCode, errorObj.Message))
                return None
        }

    member this.RegisterAsync(request: RegisterRequestDto) : Task<AuthResponseDto option> =
        task {
            let! response = httpClient.PostAsJsonAsync("/api/auth/register", request, JsonOpts.options)
            if response.IsSuccessStatusCode then
                let! authResponse = response.Content.ReadFromJsonAsync<AuthResponseDto>(JsonOpts.options)
                return Some authResponse
            else
                return None
        }

    member this.ForgotPasswordAsync(email: string) : Task<bool> =
        task {
            // Criamos o objeto an�nimo ou um mapa para serializar como JSON {"email": "..."}
            let requestBody = dict [ "Email", email ]
            let! response = httpClient.PostAsJsonAsync("/api/auth/forgot-password", requestBody, JsonOpts.options)
            return response.IsSuccessStatusCode
        }

    member this.ResetPasswordAsync(request: ResetPasswordRequestDto) : Task<bool> =
        task {
            let! response = httpClient.PostAsJsonAsync("/api/auth/reset-password", request, JsonOpts.options)
            return response.IsSuccessStatusCode
        }

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
            jindungo |> Option.iter (fun v -> url <- url + "jindungo=" + (string v).ToLower() + "&")

            let! response = httpClient.GetAsync(url)
            if response.IsSuccessStatusCode then
                let! pagedResult = response.Content.ReadFromJsonAsync<EconomiaComHistoria.Core.Helpers.PagedResult<ConteudoResponseDto>>(JsonOpts.options)
                return pagedResult
            else if (int response.StatusCode = 401) then
                return raise (ApiClientException("Unauthorized"))
            else
                return EconomiaComHistoria.Core.Helpers.PagedResult<ConteudoResponseDto>()
        }

    member this.GetConteudoAsync(id: int, ?token: string) : Task<ConteudoResponseDto option> =
        task {
            token |> Option.iter (fun t -> httpClient.DefaultRequestHeaders.Authorization <- System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", t))
            let! response = httpClient.GetAsync($"/api/conteudos/{id}")
            if response.IsSuccessStatusCode then
                let! conteudo = response.Content.ReadFromJsonAsync<ConteudoResponseDto>(JsonOpts.options)
                return Some conteudo
            else if (int response.StatusCode = 401) then
                return raise (ApiClientException("Unauthorized"))
            else
                return None
        }

    member this.SolicitarAcessoJindungoAsync(id: int, token: string) : Task<bool> =
        task {
            httpClient.DefaultRequestHeaders.Authorization <- System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token)
            let! response = httpClient.PostAsync($"/api/conteudos/{id}/solicitar-acesso", null)
            if response.IsSuccessStatusCode then
                return true
            else if (int response.StatusCode = 401) then
                return raise (ApiClientException("Unauthorized"))
            else
                return false
        }

    member this.GetSolicitacaoStatusAsync(id: int, token: string) : Task<string option> =
        task {
            httpClient.DefaultRequestHeaders.Authorization <- System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token)
            let! response = httpClient.GetAsync($"/api/conteudos/{id}/solicitacao-status")
            if response.IsSuccessStatusCode then
                let! result = response.Content.ReadFromJsonAsync<JsonElement>()
                return Some (result.GetProperty("status").GetString())
            else if (int response.StatusCode = 401) then
                return raise (ApiClientException("Unauthorized"))
            else
                return None
        }

    member this.CreateConteudoAsync(request: CreateConteudoDto, token: string) : Task<ConteudoResponseDto option> =
        task {
            this.SetAuth token
            let! response = httpClient.PostAsJsonAsync("/api/conteudos", request, JsonOpts.options)
            if response.IsSuccessStatusCode then
                let! conteudo = response.Content.ReadFromJsonAsync<ConteudoResponseDto>(JsonOpts.options)
                return Some conteudo
            else if (int response.StatusCode = 401) then
                return raise (ApiClientException("Unauthorized"))
            else
                let! errorContent = response.Content.ReadAsStringAsync()
                let errorMessage = if String.IsNullOrWhiteSpace(errorContent) then response.ReasonPhrase else errorContent
                return raise (ApiClientException(response.StatusCode, errorMessage))
        }

    member this.UpdateConteudoAsync(id: int, request: UpdateConteudoDto, token: string) : Task<ConteudoResponseDto option> =
        task {
            this.SetAuth token
            let! response = httpClient.PutAsJsonAsync($"/api/conteudos/{id}", request, JsonOpts.options)
            if response.IsSuccessStatusCode then
                let! conteudo = response.Content.ReadFromJsonAsync<ConteudoResponseDto>(JsonOpts.options)
                return Some conteudo
            else if (int response.StatusCode = 401) then
                return raise (ApiClientException("Unauthorized"))
            else
                return None
        }

    member this.DeleteConteudoAsync(id: int, token: string) : Task<bool> =
        task {
            this.SetAuth token
            let! response = httpClient.DeleteAsync($"/api/conteudos/{id}")
            if response.IsSuccessStatusCode then return true
            else if (int response.StatusCode = 401) then return raise (ApiClientException(response.StatusCode, "Unauthorized"))
            else return false
        }

    member this.UploadImagemCapaAsync(id: int, stream: System.IO.Stream, fileName: string, token: string) : Task<ConteudoResponseDto option> =
        task {
            this.SetAuth token
            use content = new MultipartFormDataContent()
            use fileContent = new StreamContent(stream)
            content.Add(fileContent, "imagem", fileName)
            let! response = httpClient.PostAsync($"/api/conteudos/{id}/imagem", content)
            if response.IsSuccessStatusCode then
                let! conteudo = response.Content.ReadFromJsonAsync<ConteudoResponseDto>(JsonOpts.options)
                return Some conteudo
            else if (int response.StatusCode = 401) then
                return raise (ApiClientException("Unauthorized"))
            else
                return None
        }

    // Quiz Methods
    member this.GetQuizDetalheAsync(id: int, token: string) : Task<QuizDetalheDto option> =
        task {
            this.SetAuth token
            let! response = httpClient.GetAsync($"/api/quizzes/{id}/detalhe")
            if response.IsSuccessStatusCode then
                let! result = response.Content.ReadFromJsonAsync<QuizDetalheDto>(JsonOpts.options)
                return Some result
            else if (int response.StatusCode = 401) then return raise (ApiClientException(response.StatusCode, "Unauthorized"))
            else return None
        }

    member this.ListQuizzesAsync(token: string, ?nivel: string, ?tema: string) : Task<QuizResponseDto list> =
        task {
            this.SetAuth token
            let mutable url = "/api/quizzes?"
            nivel |> Option.iter (fun v -> url <- url + "nivel=" + v + "&")
            tema |> Option.iter (fun v -> url <- url + "tema=" + v + "&")

            let! response = httpClient.GetAsync(url)
            if response.IsSuccessStatusCode then
                let! result = response.Content.ReadFromJsonAsync<IEnumerable<QuizResponseDto>>(JsonOpts.options)
                return List.ofSeq result
            else if (int response.StatusCode = 401) then return raise (ApiClientException(response.StatusCode, "Unauthorized"))
            else return []
        }

    member this.GetQuizStatsAsync(id: int, token: string) : Task<QuizStatsDto option> =
        task {
            this.SetAuth token
            let! response = httpClient.GetAsync($"/api/quizzes/{id}/stats")
            if response.IsSuccessStatusCode then
                let! result = response.Content.ReadFromJsonAsync<QuizStatsDto>(JsonOpts.options)
                return Some result
            else if (int response.StatusCode = 401) then return raise (ApiClientException(response.StatusCode, "Unauthorized"))
            else return None
        }

    member this.GetQuestionPoolAsync(?tema, ?nivel, ?token) : Task<PerguntaDetalheDto list> =
        task {
            token |> Option.iter this.SetAuth
            let mutable url = "/api/quizzes/pool?"
            tema |> Option.iter (fun v -> url <- url + "tema=" + v + "&")
            nivel |> Option.iter (fun v -> url <- url + "nivel=" + (string v) + "&")

            let! response = httpClient.GetAsync(url)
            if response.IsSuccessStatusCode then
                let! result = response.Content.ReadFromJsonAsync<IEnumerable<PerguntaDetalheDto>>(JsonOpts.options)
                return List.ofSeq result
            else if (int response.StatusCode = 401) then return raise (ApiClientException(response.StatusCode, "Unauthorized"))
            else return []
        }

    member this.CreateQuizAsync(request: CreateQuizDto, token: string) : Task<bool> =
        task {
            this.SetAuth token
            let! response = httpClient.PostAsJsonAsync("/api/quizzes", request, JsonOpts.options)
            if response.IsSuccessStatusCode then return true
            else if (int response.StatusCode = 401) then return raise (ApiClientException(response.StatusCode, "Unauthorized"))
            else return false
        }

    member this.UpdateQuizAsync(id: int, request: UpdateQuizDto, token: string) : Task<bool> =
        task {
            this.SetAuth token
            let! response = httpClient.PutAsJsonAsync($"/api/quizzes/{id}", request, JsonOpts.options)
            if response.IsSuccessStatusCode then return true
            else if (int response.StatusCode = 401) then return raise (ApiClientException(response.StatusCode, "Unauthorized"))
            else return false
        }

    member this.DeleteQuizAsync(id: int, token: string) : Task<bool> =
        task {
            this.SetAuth token
            let! response = httpClient.DeleteAsync($"/api/quizzes/{id}")
            if response.IsSuccessStatusCode then return true
            else if (int response.StatusCode = 401) then return raise (ApiClientException(response.StatusCode, "Unauthorized"))
            else return false
        }

    // Moderation Methods
    member this.GetPendentesAsync(token: string) : Task<ModeracaoPendentesResponse option> =
        task {
            this.SetAuth token
            let! response = httpClient.GetAsync("/api/moderacao/pendentes")
            if response.IsSuccessStatusCode then
                let! result = response.Content.ReadFromJsonAsync<ModeracaoPendentesResponse>(JsonOpts.options)
                return Some result
            else if (int response.StatusCode = 401) then return raise (ApiClientException(response.StatusCode, "Unauthorized"))
            else return None
        }

    member this.GetDenunciasAsync(token: string) : Task<DenunciaSummaryDto list> =
        task {
            this.SetAuth token
            let! response = httpClient.GetAsync("/api/moderacao/denuncias")
            if response.IsSuccessStatusCode then
                let! result = response.Content.ReadFromJsonAsync<IEnumerable<DenunciaSummaryDto>>(JsonOpts.options)
                return List.ofSeq result
            else if (int response.StatusCode = 401) then return raise (ApiClientException(response.StatusCode, "Unauthorized"))
            else return []
        }

    member this.ListUtilizadoresAsync(token: string) : Task<UtilizadorModeracaoDto list> =
        task {
            this.SetAuth token
            let! response = httpClient.GetAsync("/api/moderacao/utilizadores")
            if response.IsSuccessStatusCode then
                let! result = response.Content.ReadFromJsonAsync<IEnumerable<UtilizadorModeracaoDto>>(JsonOpts.options)
                return List.ofSeq result
            else if (int response.StatusCode = 401) then return raise (ApiClientException(response.StatusCode, "Unauthorized"))
            else return []
        }

    member this.AprovarTopicoAsync(id: int, token: string) : Task<bool> =
        task {
            this.SetAuth token
            let! response = httpClient.PutAsync($"/api/moderacao/topicos/{id}/aprovar", null)
            if response.IsSuccessStatusCode then return true
            else if (int response.StatusCode = 401) then return raise (ApiClientException(response.StatusCode, "Unauthorized"))
            else return false
        }

    member this.RejeitarTopicoAsync(id: int, request: RejeitarTopicoDto, token: string) : Task<bool> =
        task {
            this.SetAuth token
            let! response = httpClient.PutAsJsonAsync($"/api/moderacao/topicos/{id}/rejeitar", request, JsonOpts.options)
            if response.IsSuccessStatusCode then return true
            else if (int response.StatusCode = 401) then return raise (ApiClientException(response.StatusCode, "Unauthorized"))
            else return false
        }

    member this.AprovarRespostaAsync(id: int, token: string) : Task<bool> =
        task {
            this.SetAuth token
            let! response = httpClient.PutAsync($"/api/moderacao/respostas/{id}/aprovar", null)
            if response.IsSuccessStatusCode then return true
            else if (int response.StatusCode = 401) then return raise (ApiClientException(response.StatusCode, "Unauthorized"))
            else return false
        }

    member this.RejeitarRespostaAsync(id: int, request: RejeitarTopicoDto, token: string) : Task<bool> =
        task {
            this.SetAuth token
            let! response = httpClient.PutAsJsonAsync($"/api/moderacao/respostas/{id}/rejeitar", request, JsonOpts.options)
            if response.IsSuccessStatusCode then return true
            else if (int response.StatusCode = 401) then return raise (ApiClientException(response.StatusCode, "Unauthorized"))
            else return false
        }

    member this.SuspenderUtilizadorAsync(id: int, request: SuspenderUtilizadorDto, token: string) : Task<bool> =
        task {
            this.SetAuth token
            let! response = httpClient.PutAsJsonAsync($"/api/moderacao/utilizadores/{id}/suspender", request, JsonOpts.options)
            if response.IsSuccessStatusCode then return true
            else if (int response.StatusCode = 401) then return raise (ApiClientException(response.StatusCode, "Unauthorized"))
            else return false
        }

    member this.ReativarUtilizadorAsync(id: int, token: string) : Task<bool> =
        task {
            this.SetAuth token
            let! response = httpClient.PutAsync($"/api/moderacao/utilizadores/{id}/reativar", null)
            if response.IsSuccessStatusCode then return true
            else if (int response.StatusCode = 401) then return raise (ApiClientException(response.StatusCode, "Unauthorized"))
            else return false
        }

    // Gamification & Study Plan Methods
    member this.GetProgressoAsync(token: string) : Task<ProgressoUtilizadorDto option> =
        task {
            this.SetAuth token
            let! response = httpClient.GetAsync("/api/perfil/progresso")
            if response.IsSuccessStatusCode then
                let! result = response.Content.ReadFromJsonAsync<ProgressoUtilizadorDto>(JsonOpts.options)
                return Some result
            else if (int response.StatusCode = 401) then return raise (ApiClientException(response.StatusCode, "Unauthorized"))
            else return None
        }

    member this.GerarPlanoEstudoAsync(token: string) : Task<bool> =
        task {
            this.SetAuth token
            let! response = httpClient.PostAsync("/api/plano-estudo/gerar", null)
            if response.IsSuccessStatusCode then return true
            else if (int response.StatusCode = 401) then return raise (ApiClientException(response.StatusCode, "Unauthorized"))
            else return false
        }

    member this.GetBadgesAsync(token: string) : Task<BadgeConquistadoDto list> =
        task {
            this.SetAuth token
            let! response = httpClient.GetAsync("/api/moderacao/badges")
            if response.IsSuccessStatusCode then
                let! result = response.Content.ReadFromJsonAsync<IEnumerable<BadgeConquistadoDto>>(JsonOpts.options)
                return List.ofSeq result
            else if (int response.StatusCode = 401) then return raise (ApiClientException(response.StatusCode, "Unauthorized"))
            else return []
        }

    member this.GetMetricasEngajamentoAsync(token: string) : Task<JsonElement option> =
        task {
            this.SetAuth token
            let! response = httpClient.GetAsync("/api/moderacao/metricas-engajamento")
            if response.IsSuccessStatusCode then
                let! result = response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts.options)
                return Some result
            else if (int response.StatusCode = 401) then return raise (ApiClientException(response.StatusCode, "Unauthorized"))
            else return None
        }

    // Institutional Methods
    member this.ListEscolasAsync(token: string) : Task<EscolaResponseDto list> =
        task {
            this.SetAuth token
            let! response = httpClient.GetAsync("/api/escolas")
            if response.IsSuccessStatusCode then
                let! result = response.Content.ReadFromJsonAsync<IEnumerable<EscolaResponseDto>>(JsonOpts.options)
                return List.ofSeq result
            else if (int response.StatusCode = 401) then return raise (ApiClientException(response.StatusCode, "Unauthorized"))
            else return []
        }

    member this.CreateEscolaAsync(request: CreateEscolaDto, token: string) : Task<bool> =
        task {
            this.SetAuth token
            let! response = httpClient.PostAsJsonAsync("/api/escolas", request, JsonOpts.options)
            if response.IsSuccessStatusCode then return true
            else if (int response.StatusCode = 401) then return raise (ApiClientException(response.StatusCode, "Unauthorized"))
            else return false
        }

    member this.GerarCodigoConviteAsync(escolaId: int, token: string) : Task<InviteCodeResponseDto option> =
        task {
            this.SetAuth token
            let! response = httpClient.PostAsync($"/api/escolas/{escolaId}/convite", null)
            if response.IsSuccessStatusCode then
                let! result = response.Content.ReadFromJsonAsync<InviteCodeResponseDto>(JsonOpts.options)
                return Some result
            else if (int response.StatusCode = 401) then return raise (ApiClientException(response.StatusCode, "Unauthorized"))
            else return None
        }

    member this.ListTurmasAsync(token: string, ?escolaId: int) : Task<TurmaResponseDto list> =
        task {
            this.SetAuth token
            let mutable url = "/api/turmas"
            escolaId |> Option.iter (fun id -> url <- url + $"?escolaId={id}")
            let! response = httpClient.GetAsync(url)
            if response.IsSuccessStatusCode then
                let! result = response.Content.ReadFromJsonAsync<IEnumerable<TurmaResponseDto>>(JsonOpts.options)
                return List.ofSeq result
            else if (int response.StatusCode = 401) then return raise (ApiClientException(response.StatusCode, "Unauthorized"))
            else return []
        }

    member this.GetTurmaDetalheAsync(id: int, token: string) : Task<TurmaDetalheDto option> =
        task {
            this.SetAuth token
            let! response = httpClient.GetAsync($"/api/turmas/{id}")
            if response.IsSuccessStatusCode then
                let! result = response.Content.ReadFromJsonAsync<TurmaDetalheDto>(JsonOpts.options)
                return Some result
            else if (int response.StatusCode = 401) then return raise (ApiClientException(response.StatusCode, "Unauthorized"))
            else return None
        }

    member this.CreateTurmaAsync(request: CreateTurmaDto, token: string) : Task<bool> =
        task {
            this.SetAuth token
            let! response = httpClient.PostAsJsonAsync("/api/turmas", request, JsonOpts.options)
            if response.IsSuccessStatusCode then return true
            else if (int response.StatusCode = 401) then return raise (ApiClientException(response.StatusCode, "Unauthorized"))
            else return false
        }

    member this.SolicitarRelatorioAsync(request: SolicitarRelatorioDto, token: string) : Task<RelatorioStatusDto option> =
        task {
            this.SetAuth token
            let! response = httpClient.PostAsJsonAsync("/api/relatorios/gerar", request, JsonOpts.options)
            if response.IsSuccessStatusCode then
                let! result = response.Content.ReadFromJsonAsync<RelatorioStatusDto>(JsonOpts.options)
                return Some result
            else if (int response.StatusCode = 401) then return raise (ApiClientException(response.StatusCode, "Unauthorized"))
            else return None
        }

    member this.GetRelatorioStatusAsync(id: int, token: string) : Task<RelatorioStatusDto option> =
        task {
            this.SetAuth token
            let! response = httpClient.GetAsync($"/api/relatorios/{id}/status")
            if response.IsSuccessStatusCode then
                let! result = response.Content.ReadFromJsonAsync<RelatorioStatusDto>(JsonOpts.options)
                return Some result
            else if (int response.StatusCode = 401) then return raise (ApiClientException(response.StatusCode, "Unauthorized"))
            else return None
        }

    member this.GetPerfilAsync(token: string) : Task<PerfilResponseDto option> =
        task {
            this.SetAuth token
            let! response = httpClient.GetAsync("/api/perfil")
            if response.IsSuccessStatusCode then
                let! result = response.Content.ReadFromJsonAsync<PerfilResponseDto>(JsonOpts.options)
                return Some result
            else if (int response.StatusCode = 401) then return raise (ApiClientException(response.StatusCode, "Unauthorized"))
            else return None
        }

    member this.UpdatePerfilAsync(request: UpdatePerfilDto, token: string) : Task<PerfilResponseDto option> =
        task {
            this.SetAuth token
            let! response = httpClient.PutAsJsonAsync("/api/perfil", request, JsonOpts.options)
            if response.IsSuccessStatusCode then return true
            else if (int response.StatusCode = 401) then return raise (ApiClientException(response.StatusCode, "Unauthorized"))
            else return false
        }

    member this.GetRankingAsync(tipo: string, periodo: string, token: string, ?escolaId: int, ?provincia: string) : Task<RankingResponseDto option> =
        task {
            this.SetAuth token
            let mutable url = "/api/admin/auditoria?"
            utilizadorId |> Option.iter (fun v -> url <- url + "utilizadorId=" + string v + "&")
            acao |> Option.iter (fun v -> url <- url + "acao=" + v + "&")
            inicio |> Option.iter (fun (v: DateTime) -> url <- url + "inicio=" + v.ToString("o") + "&")
            fim |> Option.iter (fun (v: DateTime) -> url <- url + "fim=" + v.ToString("o") + "&")

            let! response = httpClient.GetAsync(url)
            if response.IsSuccessStatusCode then
                let! result = response.Content.ReadFromJsonAsync<IEnumerable<AuditoriaLogDto>>(JsonOpts.options)
                return List.ofSeq result
            else if (int response.StatusCode = 401) then return raise (ApiClientException(response.StatusCode, "Unauthorized"))
            else return []
        }

    member this.GetTopicoAsync(id: int) : Task<TopicoForumDetalheDto option> =
        task {
            let! response = httpClient.GetAsync($"/api/forum/topicos/{id}")
            if response.IsSuccessStatusCode then
                let! topico = response.Content.ReadFromJsonAsync<TopicoForumDetalheDto>()
                return Some topico
            else if (int response.StatusCode = 401) then
                return raise (ApiClientException("Unauthorized"))
            else
                return None
        }

    member this.CriarTopicoAsync(request: CriarTopicoForumDto, token: string) : Task<TopicoForumDto option> =
        task {
            httpClient.DefaultRequestHeaders.Authorization <- System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token)
            let! response = httpClient.PostAsJsonAsync("/api/forum/topicos", request)
            if response.IsSuccessStatusCode then
                let! topico = response.Content.ReadFromJsonAsync<TopicoForumDto>()
                return Some topico
            else if (int response.StatusCode = 401) then
                return raise (ApiClientException("Unauthorized"))
            else
                return None
        }

    member this.AdicionarRespostaAsync(topicoId: int, request: CriarRespostaForumDto, token: string) : Task<bool> =
        task {
            this.SetAuth token
            let! response = httpClient.PutAsJsonAsync($"/api/admin/utilizadores/{id}/role", request, JsonOpts.options)
            if response.IsSuccessStatusCode then return true
            else if (int response.StatusCode = 401) then return raise (ApiClientException(response.StatusCode, "Unauthorized"))
            else return false
        }