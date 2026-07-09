namespace ECHA.Web.Services

open System
open System.Net.Http
open System.Net.Http.Json
open System.Threading.Tasks
open System.Text.Json
open EconomiaComHistoria.Core.DTOs

type ApiClientException(message: string) =
    inherit Exception(message)

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
            // Criamos o objeto an�nimo ou um mapa para serializar como JSON {"email": "..."}
            let requestBody = dict [ "Email", email ]
            let! response = httpClient.PostAsJsonAsync("/api/auth/forgot-password", requestBody)
        
            // Retornamos true se a API processou (mesmo que d� 400/500, a boa pr�tica � seguir em frente na UI)
            return response.IsSuccessStatusCode
        }

    member this.ResetPasswordAsync(request: EconomiaComHistoria.Core.DTOs.ResetPasswordRequestDto) : Task<bool> =
        task {
            // Passamos o objeto 'request' diretamente. O .NET encarrega-se de gerar o JSON perfeito para a API
            let! response = httpClient.PostAsJsonAsync("/api/auth/reset-password", request)
            return response.IsSuccessStatusCode
        }

    member this.ListConteudosAsync(?tema, ?nivel, ?regiao, ?tipo, ?pagina, ?tamanho, ?estado, ?jindungo) : Task<ConteudoResponseDto list> =
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
                let! result = response.Content.ReadFromJsonAsync<JsonElement>()
                let items = JsonSerializer.Deserialize<ConteudoResponseDto list>(result.GetProperty("items").GetRawText())
                return items
            else if (int response.StatusCode = 401) then
                return raise (ApiClientException("Unauthorized"))
            else
                return []
        }

    member this.GetConteudoAsync(id: int, ?token: string) : Task<ConteudoResponseDto option> =
        task {
            token |> Option.iter (fun t -> httpClient.DefaultRequestHeaders.Authorization <- System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", t))
            let! response = httpClient.GetAsync($"/api/conteudos/{id}")
            if response.IsSuccessStatusCode then
                let! conteudo = response.Content.ReadFromJsonAsync<ConteudoResponseDto>()
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
            httpClient.DefaultRequestHeaders.Authorization <- System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token)
            let! response = httpClient.PostAsJsonAsync("/api/conteudos", request)
            if response.IsSuccessStatusCode then
                let! conteudo = response.Content.ReadFromJsonAsync<ConteudoResponseDto>()
                return Some conteudo
            else if (int response.StatusCode = 401) then
                return raise (ApiClientException("Unauthorized"))
            else
                return None
        }

    member this.UpdateConteudoAsync(id: int, request: UpdateConteudoDto, token: string) : Task<ConteudoResponseDto option> =
        task {
            httpClient.DefaultRequestHeaders.Authorization <- System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token)
            let! response = httpClient.PutAsJsonAsync($"/api/conteudos/{id}", request)
            if response.IsSuccessStatusCode then
                let! conteudo = response.Content.ReadFromJsonAsync<ConteudoResponseDto>()
                return Some conteudo
            else if (int response.StatusCode = 401) then
                return raise (ApiClientException("Unauthorized"))
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
                return raise (ApiClientException("Unauthorized"))
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
                let! conteudo = response.Content.ReadFromJsonAsync<ConteudoResponseDto>()
                return Some conteudo
            else if (int response.StatusCode = 401) then
                return raise (ApiClientException("Unauthorized"))
            else
                return None
        }

    // Quiz Methods
    member this.GetQuizDetalheAsync(id: int, token: string) : Task<QuizDetalheDto option> =
        task {
            httpClient.DefaultRequestHeaders.Authorization <- System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token)
            let! response = httpClient.GetAsync($"/api/quizzes/{id}/detalhe")
            if response.IsSuccessStatusCode then
                let! result = response.Content.ReadFromJsonAsync<QuizDetalheDto>()
                return Some result
            else if (int response.StatusCode = 401) then
                return raise (ApiClientException("Unauthorized"))
            else
                return None
        }

    member this.ListQuizzesAsync(?nivel, ?tema) : Task<QuizResponseDto list> =
        task {
            let mutable url = "/api/quizzes?"
            nivel |> Option.iter (fun v -> url <- url + "nivel=" + v + "&")
            tema |> Option.iter (fun v -> url <- url + "tema=" + v + "&")

            let! response = httpClient.GetAsync(url)
            if response.IsSuccessStatusCode then
                let! result = response.Content.ReadFromJsonAsync<QuizResponseDto list>()
                return result
            else if (int response.StatusCode = 401) then
                return raise (ApiClientException("Unauthorized"))
            else
                return []
        }

    member this.GetQuizStatsAsync(id: int, token: string) : Task<QuizStatsDto option> =
        task {
            httpClient.DefaultRequestHeaders.Authorization <- System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token)
            let! response = httpClient.GetAsync($"/api/quizzes/{id}/stats")
            if response.IsSuccessStatusCode then
                let! result = response.Content.ReadFromJsonAsync<QuizStatsDto>()
                return Some result
            else if (int response.StatusCode = 401) then
                return raise (ApiClientException("Unauthorized"))
            else
                return None
        }

    member this.GetQuestionPoolAsync(?tema, ?nivel, ?token) : Task<PerguntaStartDto list> =
        task {
            token |> Option.iter (fun t -> httpClient.DefaultRequestHeaders.Authorization <- System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", t))
            let mutable url = "/api/quizzes/pool?"
            tema |> Option.iter (fun v -> url <- url + "tema=" + v + "&")
            nivel |> Option.iter (fun v -> url <- url + "nivel=" + (string v) + "&")

            let! response = httpClient.GetAsync(url)
            if response.IsSuccessStatusCode then
                let! result = response.Content.ReadFromJsonAsync<PerguntaStartDto list>()
                return result
            else if (int response.StatusCode = 401) then
                return raise (ApiClientException("Unauthorized"))
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
                return raise (ApiClientException("Unauthorized"))
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
                return raise (ApiClientException("Unauthorized"))
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
                return raise (ApiClientException("Unauthorized"))
            else
                return false
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
                return raise (ApiClientException("Unauthorized"))
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
                return raise (ApiClientException("Unauthorized"))
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
                return raise (ApiClientException("Unauthorized"))
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
                return raise (ApiClientException("Unauthorized"))
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
                return raise (ApiClientException("Unauthorized"))
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
                return raise (ApiClientException("Unauthorized"))
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
                return raise (ApiClientException("Unauthorized"))
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
                return raise (ApiClientException("Unauthorized"))
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
                return raise (ApiClientException("Unauthorized"))
            else
                return false
        }

    // Gamification & Study Plan Methods
    member this.GetProgressoAsync(token: string) : Task<ProgressoUtilizadorDto option> =
        task {
            httpClient.DefaultRequestHeaders.Authorization <- System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token)
            let! response = httpClient.GetAsync("/api/perfil/progresso")
            if response.IsSuccessStatusCode then
                let! result = response.Content.ReadFromJsonAsync<ProgressoUtilizadorDto>()
                return Some result
            else if (int response.StatusCode = 401) then
                return raise (ApiClientException("Unauthorized"))
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
                return raise (ApiClientException("Unauthorized"))
            else
                return false
        }

    member this.GetBadgesAsync(token: string) : Task<BadgeConquistadoDto list> =
        task {
            httpClient.DefaultRequestHeaders.Authorization <- System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token)
            let! response = httpClient.GetAsync("/api/moderacao/badges") // Supondo que Admin veja todos aqui
            if response.IsSuccessStatusCode then
                let! result = response.Content.ReadFromJsonAsync<BadgeConquistadoDto list>()
                return result
            else if (int response.StatusCode = 401) then
                return raise (ApiClientException("Unauthorized"))
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
                return raise (ApiClientException("Unauthorized"))
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
                return raise (ApiClientException("Unauthorized"))
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
                return raise (ApiClientException("Unauthorized"))
            else
                return false
        }

    member this.GerarCodigoConviteAsync(escolaId: int, token: string) : Task<InviteCodeResponseDto option> =
        task {
            httpClient.DefaultRequestHeaders.Authorization <- System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token)
            let! response = httpClient.PostAsync($"/api/escolas/{escolaId}/convite", null)
            if response.IsSuccessStatusCode then
                let! result = response.Content.ReadFromJsonAsync<InviteCodeResponseDto>()
                return Some result
            else if (int response.StatusCode = 401) then
                return raise (ApiClientException("Unauthorized"))
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
                return raise (ApiClientException("Unauthorized"))
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
                return raise (ApiClientException("Unauthorized"))
            else
                return None
        }

    member this.CreateTurmaAsync(request: CreateTurmaDto, token: string) : Task<bool> =
        task {
            httpClient.DefaultRequestHeaders.Authorization <- System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token)
            let! response = httpClient.PostAsJsonAsync("/api/turmas", request)
            if response.IsSuccessStatusCode then
                return true
            else if (int response.StatusCode = 401) then
                return raise (ApiClientException("Unauthorized"))
            else
                return false
        }

    member this.SolicitarRelatorioAsync(request: SolicitarRelatorioDto, token: string) : Task<RelatorioStatusDto option> =
        task {
            httpClient.DefaultRequestHeaders.Authorization <- System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token)
            let! response = httpClient.PostAsJsonAsync("/api/relatorios/gerar", request)
            if response.IsSuccessStatusCode then
                let! result = response.Content.ReadFromJsonAsync<RelatorioStatusDto>()
                return Some result
            else if (int response.StatusCode = 401) then
                return raise (ApiClientException("Unauthorized"))
            else
                return None
        }

    member this.GetRelatorioStatusAsync(id: int, token: string) : Task<RelatorioStatusDto option> =
        task {
            httpClient.DefaultRequestHeaders.Authorization <- System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token)
            let! response = httpClient.GetAsync($"/api/relatorios/{id}/status")
            if response.IsSuccessStatusCode then
                let! result = response.Content.ReadFromJsonAsync<RelatorioStatusDto>()
                return Some result
            else if (int response.StatusCode = 401) then
                return raise (ApiClientException("Unauthorized"))
            else
                return None
        }

    member this.GetPerfilAsync(token: string) : Task<PerfilResponseDto option> =
        task {
            httpClient.DefaultRequestHeaders.Authorization <- System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token)
            let! response = httpClient.GetAsync("/api/perfil")
            if response.IsSuccessStatusCode then
                let! result = response.Content.ReadFromJsonAsync<PerfilResponseDto>()
                return Some result
            else if (int response.StatusCode = 401) then
                return raise (ApiClientException("Unauthorized"))
            else
                return None
        }

    member this.UpdatePerfilAsync(request: UpdatePerfilDto, token: string) : Task<PerfilResponseDto option> =
        task {
            httpClient.DefaultRequestHeaders.Authorization <- System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token)
            let! response = httpClient.PutAsJsonAsync("/api/perfil", request)
            if response.IsSuccessStatusCode then
                let! perfil = response.Content.ReadFromJsonAsync<PerfilResponseDto>()
                return Some perfil
            else if (int response.StatusCode = 401) then
                return raise (ApiClientException("Unauthorized"))
            else
                return None
        }

    member this.StartQuizAsync(id: int, token: string) : Task<QuizStartResponseDto option> =
        task {
            httpClient.DefaultRequestHeaders.Authorization <- System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token)
            let! response = httpClient.GetAsync($"/api/quizzes/{id}/start")
            if response.IsSuccessStatusCode then
                let! result = response.Content.ReadFromJsonAsync<QuizStartResponseDto>()
                return Some result
            else if (int response.StatusCode = 401) then
                return raise (ApiClientException("Unauthorized"))
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
                return raise (ApiClientException("Unauthorized"))
            else
                return None
        }

    member this.GetRankingAsync(tipo: string, periodo: string, token: string, ?escolaId: int, ?provincia: string) : Task<RankingResponseDto option> =
        task {
            httpClient.DefaultRequestHeaders.Authorization <- System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token)
            let mutable url = $"/api/ranking?tipo={tipo}&periodo={periodo}"
            escolaId |> Option.iter (fun id -> url <- url + $"&escolaId={id}")
            provincia |> Option.iter (fun p -> url <- url + $"&provincia={Uri.EscapeDataString(p)}")

            let! response = httpClient.GetAsync(url)
            if response.IsSuccessStatusCode then
                let! result = response.Content.ReadFromJsonAsync<RankingResponseDto>()
                return Some result
            else if (int response.StatusCode = 401) then
                return raise (ApiClientException("Unauthorized"))
            else
                return None
        }

    member this.AlterarRoleAsync(id: int, novaRole: string, token: string) : Task<bool> =
        task {
            httpClient.DefaultRequestHeaders.Authorization <- System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token)
            let body = RoleChangeDto(NovaRole = novaRole)
            let! response = httpClient.PutAsJsonAsync($"/api/admin/utilizadores/{id}/role", body)
            if response.IsSuccessStatusCode then
                return true
            else if (int response.StatusCode = 401) then
                return raise (ApiClientException("Unauthorized"))
            else
                return false
        }

    member this.ListCategoriasForumAsync() : Task<(int * string) list> =
        task {
            let! response = httpClient.GetAsync("/api/forum/categorias")
            if response.IsSuccessStatusCode then
                let! result = response.Content.ReadFromJsonAsync<JsonElement>()
                let items = JsonSerializer.Deserialize<JsonElement list>(result.GetRawText())
                return
                    items
                    |> List.map (fun item -> item.GetProperty("id").GetInt32(), item.GetProperty("nome").GetString())
            else
                return []
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
            httpClient.DefaultRequestHeaders.Authorization <- System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token)
            let! response = httpClient.PostAsJsonAsync($"/api/forum/topicos/{topicoId}/respostas", request)
            if response.IsSuccessStatusCode then
                return true
            else if (int response.StatusCode = 401) then
                return raise (ApiClientException("Unauthorized"))
            else
                return false
        }

    member this.ListTopicosAsync(?categoriaId, ?ordem) : Task<TopicoForumDto list> =
        task {
            let mutable url = "/api/forum/topicos?"
            categoriaId |> Option.iter (fun v -> url <- url + "categoriaId=" + (string v) + "&")
            ordem |> Option.iter (fun v -> url <- url + "ordem=" + v + "&")

            let! response = httpClient.GetAsync(url)
            if response.IsSuccessStatusCode then
                let! topicos = response.Content.ReadFromJsonAsync<TopicoForumDto list>()
                return topicos
            else if (int response.StatusCode = 401) then
                return raise (ApiClientException("Unauthorized"))
            else
                return []
        }