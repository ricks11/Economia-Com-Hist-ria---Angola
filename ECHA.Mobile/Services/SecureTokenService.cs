using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;

namespace ECHA.Mobile.Services;

/// <summary>
/// Implementação de <see cref="ITokenService"/> que usa o SecureStorage da plataforma
/// (Keystore no Android, Keychain no iOS) para guardar o token JWT de forma segura.
/// </summary>
public class SecureTokenService : ITokenService
{
    private readonly string _storageKey;

    public SecureTokenService(IConfiguration configuration)
    {
        // Lê a chave de armazenamento das configurações (appsettings.Production.json)
        _storageKey = configuration["Jwt:StorageKey"] ?? "echa_jwt_token";
    }

    /// <inheritdoc/>
    public async Task SaveTokenAsync(string token)
    {
        await SecureStorage.Default.SetAsync(_storageKey, token);
    }

    /// <inheritdoc/>
    public async Task<string?> GetTokenAsync()
    {
        try
        {
            var token = await SecureStorage.Default.GetAsync(_storageKey);

            if (string.IsNullOrWhiteSpace(token))
                return null;

            // Valida localmente a expiração do token sem contactar o servidor
            if (IsTokenExpired(token))
            {
                await RemoveTokenAsync();
                return null;
            }

            return token;
        }
        catch
        {
            // SecureStorage pode lançar excepção em simuladores ou configurações inválidas
            return null;
        }
    }

    /// <inheritdoc/>
    public Task RemoveTokenAsync()
    {
        SecureStorage.Default.Remove(_storageKey);
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public async Task<bool> IsAuthenticatedAsync()
    {
        var token = await GetTokenAsync();
        return !string.IsNullOrEmpty(token);
    }

    // ─────────────────────────────────────────
    // HELPERS PRIVADOS
    // ─────────────────────────────────────────

    /// <summary>
    /// Descodifica o JWT localmente (sem validação de assinatura) e verifica a expiração.
    /// Evita chamadas desnecessárias à API com tokens já expirados.
    /// </summary>
    private static bool IsTokenExpired(string token)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            // Adiciona 30 segundos de margem para compensar diferenças de relógio
            return jwtToken.ValidTo < DateTime.UtcNow.AddSeconds(30);
        }
        catch
        {
            // Token malformado → trata como expirado
            return true;
        }
    }
}
