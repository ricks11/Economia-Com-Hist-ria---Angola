using EconomiaComHistoria.Core.Enums;

namespace EconomiaComHistoria.Core.DTOs;

public record CreateQuizDto(
    string Titulo,
    string Tema,
    NivelDificuldade Nivel,
    int TotalPerguntas,
    int TempoLimiteSeg,
    List<CreatePerguntaDto> Perguntas
);

public record CreatePerguntaDto(
    string Enunciado,
    string Tema,
    NivelDificuldade Dificuldade,
    List<CreateOpcaoRespostaDto> Opcoes,
    string Explicacao);

public record CreateOpcaoRespostaDto(
    string Texto,
    bool IsCorrecta,
    string? Explicacao);

public record UpdateQuizDto(
    string Titulo,
    string Tema,
    NivelDificuldade Nivel,
    int TotalPerguntas,
    int TempoLimiteSeg
);

public record QuizResponseDto(
    int Id,
    string Titulo,
    string Tema,
    NivelDificuldade Nivel,
    int TotalPerguntas,
    int TempoLimiteSeg,
    bool Ativo
);

public record QuizStartResponseDto(
    int TentativaId,
    List<PerguntaStartDto> Perguntas
);

public record PerguntaStartDto(
    int Id,
    string Enunciado,
    List<OpcaoRespostaStartDto> Opcoes);

public record OpcaoRespostaStartDto(int Id, string Texto);

public record SubmitTentativaDto(
    int TentativaId,
    List<RespostaPerguntaDto> Respostas
);

public record RespostaPerguntaDto(int PerguntaId, int OpcaoRespostaId, int TempoRespostaSeg);

public record QuizSubmissionResponseDto(
    int Pontuacao,
    double Percentagem,
    List<RespostaDetalhadaDto> Respostas
);

public record RespostaDetalhadaDto(
    int PerguntaId,
    string Enunciado,
    int OpcaoEscolhida,
    string TextoOpcaoEscolhida,
    bool Correta,
    string Explicacao
);

public record QuizResultDto(
    int QuizId,
    int UtilizadorId,
    int Pontuacao,
    int BonusVelocidade,
    int TempoGastoSeg,
    float PercentagemAcerto,
    int TotalPerguntas,
    int TotalCorretas,
    bool ElegivelParaRanking);
