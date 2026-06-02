namespace EconomiaComHistoria.API.DTOs;

public record AuthResponseDto(
    int Id,
    string Email,
    string Nome,
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresIn);
