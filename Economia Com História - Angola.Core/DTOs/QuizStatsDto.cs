namespace EconomiaComHistoria.Core.DTOs;

public record QuestionStatsDto(
    int PerguntaId,
    string Texto,
    int TotalTentativas,
    double TaxaAcerto,
    double TempoMedioMs
);

public record QuizStatsDto(
    int QuizId,
    string Titulo,
    int TotalTentativas,
    List<QuestionStatsDto> Perguntas
);
