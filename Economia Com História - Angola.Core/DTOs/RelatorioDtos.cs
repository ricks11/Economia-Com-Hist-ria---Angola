using EconomiaComHistoria.Core.Enums;

namespace EconomiaComHistoria.Core.DTOs;

public record SolicitarRelatorioDto(
    string Titulo,
    string Tipo, // PDF, CSV
    int? TurmaId,
    int? EscolaId,
    DateTime? Inicio,
    DateTime? Fim);

public record RelatorioStatusDto(
    int Id,
    string Titulo,
    EstadoRelatorio Estado,
    DateTime DataSolicitacao,
    DateTime? DataConclusao,
    string? DownloadUrl);
