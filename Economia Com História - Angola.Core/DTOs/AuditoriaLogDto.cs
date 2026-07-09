namespace EconomiaComHistoria.Core.DTOs;

public record AuditoriaLogDto(
    int Id,
    int? UtilizadorId,
    string? UtilizadorNome,
    string Acao,
    string? EntidadeAfetada,
    int? IdEntidade,
    string? ValorAntes,
    string? ValorDepois,
    string? Ip,
    DateTime Timestamp,
    string Resultado
);
