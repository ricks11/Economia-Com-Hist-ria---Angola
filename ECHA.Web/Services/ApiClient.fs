namespace ECHA.Web.Services

open System.Net.Http
open System.Net.Http.Json
open System.Threading.Tasks
open ECHA.Core.DTOs

type ApiClient(httpClient: HttpClient) =
    member this.LoginAsync(request: LoginRequestDto) : Task<string option> =
        task {
            let! response = httpClient.PostAsJsonAsync("/api/auth/login", request)
            if response.IsSuccessStatusCode then
                let! token = response.Content.ReadFromJsonAsync<string>()
                return Some token
            else
                return None
        }
