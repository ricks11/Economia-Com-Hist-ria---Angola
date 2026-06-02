using EconomiaComHistoria.Core.Enums;

namespace EconomiaComHistoria.Core.DTOs;

public record PerfilResponseDto(
    int Id,
    string Nome,
    string Email,
    string? Telemovel,
    TipoUtilizador Tipo,
    DateTime DataRegisto,
    int PontosTotais,
    int StreakAtual,
    string? Provincia,
    int? EscolaId,
    string? EscolaNome,
    int? TurmaId,
    string? TurmaNome);
