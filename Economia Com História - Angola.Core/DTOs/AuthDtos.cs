namespace EconomiaComHistoria.Core.DTOs;

public record LoginRequestDto(string Email, string Password);

public record RegisterRequestDto(string Email, string Password, string Nome, string? Telemovel);

public record ForgotPasswordRequestDto(string Email);

public record ResetPasswordRequestDto(string Email, string Token, string NewPassword);

public record RefreshTokenRequestDto(string RefreshToken);

public record AuthResponseDto(
    int Id, 
    string Email, 
    string Nome, 
    string AccessToken, 
    string RefreshToken, 
    DateTime ExpiresIn, 
    string Tipo);
