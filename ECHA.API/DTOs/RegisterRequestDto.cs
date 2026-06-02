namespace EconomiaComHistoria.API.DTOs;

public record RegisterRequestDto(
    string Email,
    string Password,
    string Nome,
    string? Telemovel);
