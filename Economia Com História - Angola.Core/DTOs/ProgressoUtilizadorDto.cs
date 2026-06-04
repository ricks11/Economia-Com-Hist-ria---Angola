namespace EconomiaComHistoria.Core.DTOs;

public record BadgeConquistadoDto(
    int Id,
    string Nome,
    string? Descricao,
    string? Icone,
    DateTime DataConquista
);

public record ProgressoUtilizadorDto(
    int PontosTotais,
    int Nivel,
    int PontosParaProximoNivel,
    double PercentagemNivel,
    int StreakAtual,
    List<BadgeConquistadoDto> Badges
);
