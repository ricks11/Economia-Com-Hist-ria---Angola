namespace EconomiaComHistoria.Core.DTOs.Sync;

public record ConteudoTraducaoOfflineDto(string Lingua, string? Texto, string? AudioUrl);

public record ConteudoOfflinePacoteDto(
    int Id,
    string Titulo,
    string Resumo,
    string Tipo,
    string Tema,
    string? UrlFicheiro,
    string? ThumbnailUrl,
    int? DuracaoSegundos,
    string NivelDificuldade,
    bool IsJindungo,
    string? ReferenciaFactual,
    List<ConteudoTraducaoOfflineDto> Traducoes,
    DateTime GeradoEm
);