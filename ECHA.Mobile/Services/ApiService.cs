using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace ECHA.Mobile.Services;

public interface IApiService
{
    // ── Chamadas públicas (sem token) ──────────────────────────────────────
    Task<T?> GetAsync<T>(string endpoint);
    Task<TResponse?> PostAsync<TRequest, TResponse>(string endpoint, TRequest data);

    // ── Chamadas autenticadas (injetam Bearer token automaticamente) ────────
    Task<T?> AuthGetAsync<T>(string endpoint);
    Task<TResponse?> AuthPostAsync<TRequest, TResponse>(string endpoint, TRequest data);
    Task<TResponse?> AuthPutAsync<TRequest, TResponse>(string endpoint, TRequest data);
    Task AuthDeleteAsync(string endpoint);
}

/// <summary>
/// Serviço HTTP central da app mobile.
/// • Chamadas <c>Get/Post</c> são públicas (sem autenticação).
/// • Chamadas <c>AuthGet/AuthPost/AuthPut/AuthDelete</c> injetam automaticamente
///   o token JWT no cabeçalho <c>Authorization: Bearer …</c>.
/// </summary>
public class ApiService : IApiService
{
    private readonly HttpClient _httpClient;
    private readonly ITokenService _tokenService;
    private readonly JsonSerializerOptions _serializerOptions;

    public ApiService(HttpClient httpClient, ITokenService tokenService)
    {
        _httpClient = httpClient;
        _tokenService = tokenService;
        _serializerOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    // ─────────────────────────────────────────────────────────────────────
    // CHAMADAS PÚBLICAS
    // ─────────────────────────────────────────────────────────────────────

    public async Task<T?> GetAsync<T>(string endpoint)
    {
        var response = await _httpClient.GetAsync(endpoint);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<T>(_serializerOptions);
    }

    public async Task<TResponse?> PostAsync<TRequest, TResponse>(string endpoint, TRequest data)
    {
        var response = await _httpClient.PostAsJsonAsync(endpoint, data, _serializerOptions);
        await EnsureSuccessAsync(response);
        return await response.Content.ReadFromJsonAsync<TResponse>(_serializerOptions);
    }

    // ─────────────────────────────────────────────────────────────────────
    // CHAMADAS AUTENTICADAS  (Bearer token injetado automaticamente)
    // ─────────────────────────────────────────────────────────────────────

    public async Task<T?> AuthGetAsync<T>(string endpoint)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
        await AttachBearerTokenAsync(request);
        var response = await _httpClient.SendAsync(request);
        await EnsureSuccessAsync(response);
        return await response.Content.ReadFromJsonAsync<T>(_serializerOptions);
    }

    public async Task<TResponse?> AuthPostAsync<TRequest, TResponse>(string endpoint, TRequest data)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
        request.Content = JsonContent.Create(data, options: _serializerOptions);
        await AttachBearerTokenAsync(request);
        var response = await _httpClient.SendAsync(request);
        await EnsureSuccessAsync(response);
        return await response.Content.ReadFromJsonAsync<TResponse>(_serializerOptions);
    }

    public async Task<TResponse?> AuthPutAsync<TRequest, TResponse>(string endpoint, TRequest data)
    {
        using var request = new HttpRequestMessage(HttpMethod.Put, endpoint);
        request.Content = JsonContent.Create(data, options: _serializerOptions);
        await AttachBearerTokenAsync(request);
        var response = await _httpClient.SendAsync(request);
        await EnsureSuccessAsync(response);
        return await response.Content.ReadFromJsonAsync<TResponse>(_serializerOptions);
    }

    public async Task AuthDeleteAsync(string endpoint)
    {
        using var request = new HttpRequestMessage(HttpMethod.Delete, endpoint);
        await AttachBearerTokenAsync(request);
        var response = await _httpClient.SendAsync(request);
        await EnsureSuccessAsync(response);
    }

    // ─────────────────────────────────────────────────────────────────────
    // HELPERS PRIVADOS
    // ─────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Lê o token do SecureStorage e injeta-o no cabeçalho Authorization da requisição.
    /// Se não existir token, lança UnauthorizedAccessException para que o chamador
    /// possa redirecionar para o ecrã de login.
    /// </summary>
    private async Task AttachBearerTokenAsync(HttpRequestMessage request)
    {
        var token = await _tokenService.GetTokenAsync();
        if (string.IsNullOrEmpty(token))
            throw new UnauthorizedAccessException("Sessão expirada. Por favor, faça login novamente.");

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    /// <summary>
    /// Verifica o código de resposta e lança excepções tipificadas para tratamento
    /// consistente em toda a app (401 → redireciona para login, etc.).
    /// </summary>
    private static async Task EnsureSuccessAsync(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode) return;

        var body = await response.Content.ReadAsStringAsync();

        if (response.StatusCode == HttpStatusCode.Unauthorized)
            throw new UnauthorizedAccessException("Sessão inválida ou expirada.");

        if (response.StatusCode == HttpStatusCode.Forbidden)
            throw new UnauthorizedAccessException("Não tem permissão para realizar esta acção.");

        throw new HttpRequestException($"API Error {(int)response.StatusCode}: {body}");
    }
}

