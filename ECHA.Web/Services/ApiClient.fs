namespace ECHA.Web.Services

open System.Net.Http
open System.Net.Http.Json
open System.Threading.Tasks
open ECHA.Core.DTOs

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

    member this.ListConteudosAsync(?tema, ?nivel, ?regiao, ?tipo, ?pagina, ?tamanho) : Task<ConteudoResponseDto list> =
        task {
            let mutable url = "/api/conteudos?"
            tema |> Option.iter (fun v -> url <- url + "tema=" + v + "&")
            nivel |> Option.iter (fun v -> url <- url + "nivel=" + v + "&")
            regiao |> Option.iter (fun v -> url <- url + "regiao=" + v + "&")
            tipo |> Option.iter (fun v -> url <- url + "tipo=" + v + "&")
            pagina |> Option.iter (fun v -> url <- url + "pagina=" + (string v) + "&")
            tamanho |> Option.iter (fun v -> url <- url + "tamanho=" + (string v) + "&")

            let! response = httpClient.GetAsync(url)
            if response.IsSuccessStatusCode then
                let! result = response.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>()
                let items = result.GetProperty("items").Deserialize<ConteudoResponseDto list>()
                return items
            else
                return []
        }

    member this.GetConteudoAsync(id: int) : Task<ConteudoResponseDto option> =
        task {
            let! response = httpClient.GetAsync($"/api/conteudos/{id}")
            if response.IsSuccessStatusCode then
                let! conteudo = response.Content.ReadFromJsonAsync<ConteudoResponseDto>()
                return Some conteudo
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
            else
                return None
        }

    member this.DeleteConteudoAsync(id: int, token: string) : Task<bool> =
        task {
            httpClient.DefaultRequestHeaders.Authorization <- System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token)
            let! response = httpClient.DeleteAsync($"/api/conteudos/{id}")
            return response.IsSuccessStatusCode
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
            else
                return None
        }

    // Quiz Methods
    member this.ListQuizzesAsync(?nivel, ?tema) : Task<QuizResponseDto list> =
        task {
            let mutable url = "/api/quizzes?"
            nivel |> Option.iter (fun v -> url <- url + "nivel=" + v + "&")
            tema |> Option.iter (fun v -> url <- url + "tema=" + v + "&")

            let! response = httpClient.GetAsync(url)
            if response.IsSuccessStatusCode then
                let! result = response.Content.ReadFromJsonAsync<QuizResponseDto list>()
                return result
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
            else
                return []
        }

    member this.CreateQuizAsync(request: CreateQuizDto, token: string) : Task<bool> =
        task {
            httpClient.DefaultRequestHeaders.Authorization <- System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token)
            let! response = httpClient.PostAsJsonAsync("/api/quizzes", request)
            return response.IsSuccessStatusCode
        }

    member this.UpdateQuizAsync(id: int, request: UpdateQuizDto, token: string) : Task<bool> =
        task {
            httpClient.DefaultRequestHeaders.Authorization <- System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token)
            let! response = httpClient.PutAsJsonAsync($"/api/quizzes/{id}", request)
            return response.IsSuccessStatusCode
        }

    member this.DeleteQuizAsync(id: int, token: string) : Task<bool> =
        task {
            httpClient.DefaultRequestHeaders.Authorization <- System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token)
            let! response = httpClient.DeleteAsync($"/api/quizzes/{id}")
            return response.IsSuccessStatusCode
        }

    // Moderation Methods
    member this.GetPendentesAsync(token: string) : Task<ModeracaoPendentesResponse option> =
        task {
            httpClient.DefaultRequestHeaders.Authorization <- System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token)
            let! response = httpClient.GetAsync("/api/moderacao/pendentes")
            if response.IsSuccessStatusCode then
                let! result = response.Content.ReadFromJsonAsync<ModeracaoPendentesResponse>()
                return Some result
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
            else
                return []
        }

    member this.AprovarTopicoAsync(id: int, token: string) : Task<bool> =
        task {
            httpClient.DefaultRequestHeaders.Authorization <- System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token)
            let! response = httpClient.PutAsync($"/api/moderacao/topicos/{id}/aprovar", null)
            return response.IsSuccessStatusCode
        }

    member this.RejeitarTopicoAsync(id: int, request: RejeitarTopicoDto, token: string) : Task<bool> =
        task {
            httpClient.DefaultRequestHeaders.Authorization <- System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token)
            let! response = httpClient.PutAsJsonAsync($"/api/moderacao/topicos/{id}/rejeitar", request)
            return response.IsSuccessStatusCode
        }

    member this.AprovarRespostaAsync(id: int, token: string) : Task<bool> =
        task {
            httpClient.DefaultRequestHeaders.Authorization <- System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token)
            let! response = httpClient.PutAsync($"/api/moderacao/respostas/{id}/aprovar", null)
            return response.IsSuccessStatusCode
        }

    member this.RejeitarRespostaAsync(id: int, request: RejeitarTopicoDto, token: string) : Task<bool> =
        task {
            httpClient.DefaultRequestHeaders.Authorization <- System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token)
            let! response = httpClient.PutAsJsonAsync($"/api/moderacao/respostas/{id}/rejeitar", request)
            return response.IsSuccessStatusCode
        }

    member this.SuspenderUtilizadorAsync(id: int, request: SuspenderUtilizadorDto, token: string) : Task<bool> =
        task {
            httpClient.DefaultRequestHeaders.Authorization <- System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token)
            let! response = httpClient.PutAsJsonAsync($"/api/moderacao/utilizadores/{id}/suspender", request)
            return response.IsSuccessStatusCode
        }

    member this.ReativarUtilizadorAsync(id: int, token: string) : Task<bool> =
        task {
            httpClient.DefaultRequestHeaders.Authorization <- System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token)
            let! response = httpClient.PutAsync($"/api/moderacao/utilizadores/{id}/reativar", null)
            return response.IsSuccessStatusCode
        }

    // Gamification & Study Plan Methods
    member this.GetProgressoAsync(token: string) : Task<ProgressoUtilizadorDto option> =
        task {
            httpClient.DefaultRequestHeaders.Authorization <- System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token)
            let! response = httpClient.GetAsync("/api/perfil/progresso")
            if response.IsSuccessStatusCode then
                let! result = response.Content.ReadFromJsonAsync<ProgressoUtilizadorDto>()
                return Some result
            else
                return None
        }

    member this.GerarPlanoEstudoAsync(token: string) : Task<bool> =
        task {
            httpClient.DefaultRequestHeaders.Authorization <- System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token)
            let! response = httpClient.PostAsync("/api/plano-estudo/gerar", null)
            return response.IsSuccessStatusCode
        }

    member this.GetBadgesAsync(token: string) : Task<Badge list> =
        task {
            httpClient.DefaultRequestHeaders.Authorization <- System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token)
            let! response = httpClient.GetAsync("/api/moderacao/badges") // Supondo que Admin veja todos aqui
            if response.IsSuccessStatusCode then
                let! result = response.Content.ReadFromJsonAsync<Badge list>()
                return result
            else
                return []
        }

    member this.GetMetricasEngajamentoAsync(token: string) : Task<System.Text.Json.JsonElement option> =
        task {
            httpClient.DefaultRequestHeaders.Authorization <- System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token)
            let! response = httpClient.GetAsync("/api/moderacao/metricas-engajamento")
            if response.IsSuccessStatusCode then
                let! result = response.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>()
                return Some result
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
            else
                return []
        }

    member this.CreateEscolaAsync(request: CreateEscolaDto, token: string) : Task<bool> =
        task {
            httpClient.DefaultRequestHeaders.Authorization <- System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token)
            let! response = httpClient.PostAsJsonAsync("/api/escolas", request)
            return response.IsSuccessStatusCode
        }

    member this.GerarCodigoConviteAsync(escolaId: int, token: string) : Task<InviteCodeResponseDto option> =
        task {
            httpClient.DefaultRequestHeaders.Authorization <- System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token)
            let! response = httpClient.PostAsync($"/api/escolas/{escolaId}/convite", null)
            if response.IsSuccessStatusCode then
                let! result = response.Content.ReadFromJsonAsync<InviteCodeResponseDto>()
                return Some result
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
            else
                return None
        }

    member this.CreateTurmaAsync(request: CreateTurmaDto, token: string) : Task<bool> =
        task {
            httpClient.DefaultRequestHeaders.Authorization <- System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token)
            let! response = httpClient.PostAsJsonAsync("/api/turmas", request)
            return response.IsSuccessStatusCode
        }

    member this.SolicitarRelatorioAsync(request: SolicitarRelatorioDto, token: string) : Task<RelatorioStatusDto option> =
        task {
            httpClient.DefaultRequestHeaders.Authorization <- System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token)
            let! response = httpClient.PostAsJsonAsync("/api/relatorios/gerar", request)
            if response.IsSuccessStatusCode then
                let! result = response.Content.ReadFromJsonAsync<RelatorioStatusDto>()
                return Some result
            else
                return None
        }
