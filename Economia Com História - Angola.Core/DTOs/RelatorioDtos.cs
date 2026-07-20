using EconomiaComHistoria.Core.Enums;

namespace EconomiaComHistoria.Core.DTOs;

public record SolicitarRelatorioDto(
    string Titulo,
    string Tipo, // PDF, CSV
    int? TurmaId,
    int? EscolaId,
    DateTime? Inicio,
    DateTime? Fim
);

public record RelatorioStatusDto(
    int Id,
    string Titulo,
    string Tipo,
    EstadoRelatorio Estado,
    DateTime DataSolicitacao,
    DateTime? DataConclusao,
    string? DownloadUrl,
    string? MensagemErro = null
);

public record RelatorioListaDto(
    int Id,
    string Titulo,
    string Tipo,
    EstadoRelatorio Estado,
    DateTime DataSolicitacao,
    DateTime? DataConclusao
);