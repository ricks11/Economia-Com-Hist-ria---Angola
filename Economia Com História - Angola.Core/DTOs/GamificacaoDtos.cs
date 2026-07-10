namespace EconomiaComHistoria.Core.DTOs;

public record ProgressoProvinciaDto(string NomeProvincia, double PercentualExplorado);

public record SugestaoEstudoDto(string Titulo, int ConteudoId, string Prioridade);

public record UserStatsDto(
    int Nivel,
    int XPAtual,
    int XPProximoNivel,
    int StreakDias,
    List<BadgeConquistadoDto> Badges,
    List<ProgressoProvinciaDto> ProgressoProvincias
);

public record BadgeAdminDto(
    int Id,
    string Nome,
    string Descricao,
    string? Icone,
    string CriterioTipo,
    int CriterioValor,
    int TotalConquistados
);
