namespace EconomiaComHistoria.Core.DTOs.Sync;

public record RespostaOfflineDto(int PerguntaId, int IndiceSelecionado, int TempoRespostaSeg);

public record TentativaOfflineDto(
    Guid IdLocal,
    int QuizId,
    DateTime DataRealizacaoCliente,
    int TempoGastoSeg,
    List<RespostaOfflineDto> Respostas
);

public record LoteSincronizacaoRequest(List<TentativaOfflineDto> Tentativas);

public record ResultadoSincronizacaoItem(
    Guid IdLocal,
    int? IdTentativaServidor,
    bool Aceite,
    bool ElegivelRanking,
    string? MotivoRejeicao
);

public record LoteSincronizacaoResponse(List<ResultadoSincronizacaoItem> Resultados);