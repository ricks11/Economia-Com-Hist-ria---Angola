namespace ECHA.Mobile.Services;

/// <summary>
/// Contrato para gerir o token JWT da sessão do utilizador.
/// Utiliza o Secure Storage nativo (Keystore Android / Keychain iOS).
/// </summary>
public interface ITokenService
{
    /// <summary>Guarda o token de forma segura após login.</summary>
    Task SaveTokenAsync(string token);

    /// <summary>Devolve o token guardado, ou null se não existir / estiver expirado.</summary>
    Task<string?> GetTokenAsync();

    /// <summary>Elimina o token (logout).</summary>
    Task RemoveTokenAsync();

    /// <summary>Indica se existe um token válido guardado.</summary>
    Task<bool> IsAuthenticatedAsync();
}
