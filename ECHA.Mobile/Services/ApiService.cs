using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace ECHA.Mobile.Services;

/// <summary>
/// Contrato do serviço HTTP central da app mobile.
/// O token JWT é gerido automaticamente pelo <see cref="AuthHeaderHandler"/> —
/// não é necessário distinguir chamadas "públicas" de "autenticadas" na interface.
/// </summary>
public interface IApiService
{
    Task<T?> GetAsync<T>(string endpoint);
    Task<TResponse?> PostAsync<TRequest, TResponse>(string endpoint, TRequest data);
    Task<TResponse?> PutAsync<TRequest, TResponse>(string endpoint, TRequest data);
    Task DeleteAsync(string endpoint);
}

/// <summary>
/// Implementação do cliente HTTP da app mobile.
///
/// Responsabilidade única: serialização / deserialização de pedidos HTTP.
/// A injecção do token Bearer e o redirecionamento em caso de sessão expirada
/// são tratados globalmente pelo <see cref="AuthHeaderHandler"/>, que actua
/// como middleware da camada de transporte.
/// </summary>
public class ApiService : IApiService
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _serializerOptions;

    public ApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _serializerOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    // ─────────────────────────────────────────────────────────────────────
    // MÉTODOS HTTP  (token injectado automaticamente pelo AuthHeaderHandler)
    // ─────────────────────────────────────────────────────────────────────

    public async Task<T?> GetAsync<T>(string endpoint)
    {
        var response = await _httpClient.GetAsync(endpoint);
        await EnsureSuccessAsync(response);
        return await response.Content.ReadFromJsonAsync<T>(_serializerOptions);
    }

    public async Task<TResponse?> PostAsync<TRequest, TResponse>(string endpoint, TRequest data)
    {
        var response = await _httpClient.PostAsJsonAsync(endpoint, data, _serializerOptions);
        await EnsureSuccessAsync(response);
        return await response.Content.ReadFromJsonAsync<TResponse>(_serializerOptions);
    }

    public async Task<TResponse?> PutAsync<TRequest, TResponse>(string endpoint, TRequest data)
    {
        var response = await _httpClient.PutAsJsonAsync(endpoint, data, _serializerOptions);
        await EnsureSuccessAsync(response);
        return await response.Content.ReadFromJsonAsync<TResponse>(_serializerOptions);
    }

    public async Task DeleteAsync(string endpoint)
    {
        var response = await _httpClient.DeleteAsync(endpoint);
        await EnsureSuccessAsync(response);
    }

    // ─────────────────────────────────────────────────────────────────────
    // HELPER PRIVADO
    // ─────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Verifica o código de resposta e lança excepções tipificadas.
    /// Nota: o 401 também é tratado pelo AuthHeaderHandler para redirecionar
    /// para o Login — este método lança excepção adicional para PageModels
    /// que queiram tratar o erro localmente (ex: mostrar mensagem).
    /// </summary>
    private static async Task EnsureSuccessAsync(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode) return;

        var body = await response.Content.ReadAsStringAsync();

        throw response.StatusCode switch
        {
            HttpStatusCode.Unauthorized  => new UnauthorizedAccessException("Sessão inválida ou expirada."),
            HttpStatusCode.Forbidden     => new UnauthorizedAccessException("Não tem permissão para realizar esta acção."),
            HttpStatusCode.NotFound      => new KeyNotFoundException($"Recurso não encontrado: {response.RequestMessage?.RequestUri?.PathAndQuery}"),
            HttpStatusCode.TooManyRequests => new InvalidOperationException("Demasiados pedidos. Aguarde um momento e tente novamente."),
            _                            => new HttpRequestException($"Erro da API [{(int)response.StatusCode}]: {body}")
        };
    }
}
