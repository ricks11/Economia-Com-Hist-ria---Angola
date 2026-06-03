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
