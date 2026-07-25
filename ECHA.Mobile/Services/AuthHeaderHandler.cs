using System.Net;
using System.Net.Http.Headers;

namespace ECHA.Mobile.Services;

/// <summary>
/// DelegatingHandler que actua como middleware da camada HTTP da app.
///
/// Responsabilidades:
///   1. Injectar automaticamente o token JWT no cabeçalho Authorization
///      de todos os pedidos de saída (se existir token guardado).
///   2. Interceptar respostas 401 Unauthorized, limpar a sessão expirada
///      e redirecionar o utilizador para o ecrã de Login de forma limpa.
///
/// Esta classe elimina a necessidade de lógica de token em ApiService
/// ou em qualquer PageModel — toda a gestão de sessão é centralizada aqui.
/// </summary>
public class AuthHeaderHandler : DelegatingHandler
{
    private readonly ITokenService _tokenService;

    // Guarda de reentrância: evita loop infinito se o redirecionamento
    // para Login também devolver 401 (ex: bug no servidor).
    private bool _isRedirecting = false;

    public AuthHeaderHandler(ITokenService tokenService)
    {
        _tokenService = tokenService;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        // ── 1. Injectar o token (se existir) ──────────────────────────────
        var token = await _tokenService.GetTokenAsync();

        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        // ── 2. Enviar o pedido para a API ──────────────────────────────────
        var response = await base.SendAsync(request, cancellationToken);

        // ── 3. Interceptar sessão expirada / inválida ──────────────────────
        if (response.StatusCode == HttpStatusCode.Unauthorized && !_isRedirecting)
        {
            _isRedirecting = true;

            // Remove o token inválido do SecureStorage (Keystore/Keychain)
            await _tokenService.RemoveTokenAsync();

            // Navega para o Login na thread de UI (obrigatório para operações visuais)
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                try
                {
                    // Shell.GoToAsync é o método correcto para apps MAUI Shell.
                    // '//LoginPage' com '//' é uma navegação absoluta (limpa a pilha).
                    await Shell.Current.GoToAsync("//LoginPage");
                }
                catch
                {
                    // Fallback: se a Shell ainda não estiver inicializada
                    // (ex: em testes ou arranque muito rápido), não faz nada.
                }
                finally
                {
                    _isRedirecting = false;
                }
            });
        }

        return response;
    }
}
