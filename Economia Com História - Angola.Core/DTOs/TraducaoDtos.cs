namespace EconomiaComHistoria.Core.DTOs;

public record CreateTraducaoDto(
    string Lingua,
    string? Texto,
    string? AudioUrl);

public record TraducaoResponseDto(
    int Id,
    string Lingua,
    string? Texto,
    string? AudioUrl);
