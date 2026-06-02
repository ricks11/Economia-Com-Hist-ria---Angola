namespace EconomiaComHistoria.Core.DTOs;

public record UpdatePerfilDto(
    string? Nome,
    string? Provincia,
    int? EscolaId,
    int? TurmaId);
