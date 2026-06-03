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
