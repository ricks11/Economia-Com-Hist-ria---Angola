namespace EconomiaComHistoria.API.Services;

public interface IAuthService
{
    /// <summary>
    /// Hashes a password using BCrypt.
    /// </summary>
    string HashPassword(string password);

    /// <summary>
    /// Verifies a password against its BCrypt hash.
    /// </summary>
    bool VerifyPassword(string password, string hash);

    /// <summary>
    /// Generates a JWT access token.
    /// </summary>
    string GenerateAccessToken(int userId, string email, string role);

    /// <summary>
    /// Generates a refresh token (simple GUID-based token).
    /// </summary>
    string GenerateRefreshToken();

    /// <summary>
    /// Gets the access token expiration time.
    /// </summary>
    DateTime GetAccessTokenExpiration();
}
