namespace EconomiaComHistoria.API.DTOs;

public record CreateQuizDto(
    string Titulo,
    string? Descricao,
    int NivelDificuldade,
    string Tema,
    int NumeroPerguntas,
    int TempoPorPerguntaSegundos,
    List<CreatePerguntaDto> Perguntas
);

public record CreatePerguntaDto(
    string Texto,
    int TempoLimiteSegundos,
    List<CreateOpcaoDto> Opcoes
);

public record CreateOpcaoDto(
    string Texto,
    bool IsCorrecta,
    string? Explicacao
);

public record UpdateQuizDto(
    string Titulo,
    string? Descricao,
    int NivelDificuldade,
    string Tema,
    int NumeroPerguntas,
    int TempoPorPerguntaSegundos
);

public record QuizResponseDto(
    int Id,
    string Titulo,
    string? Descricao,
    int NivelDificuldade,
    string Tema,
    int NumeroPerguntas,
    int TempoPorPerguntaSegundos
);

public record QuizStartResponseDto(
    int TentativaId,
    List<PerguntaStartDto> Perguntas
);

public record PerguntaStartDto(
    int Id,
    string Texto,
    List<OpcaoStartDto> Opcoes
);

public record OpcaoStartDto(
    int Id,
    string Texto
);

public record SubmitTentativaDto(
    int TentativaId,
    List<RespostaPerguntaDto> Respostas
);

public record RespostaPerguntaDto(
    int PerguntaId,
    int OpcaoId,
    int TempoMs
);

public record QuizSubmissionResponseDto(
    int Pontuacao,
    double Percentagem,
    List<RespostaDetalhadaDto> Respostas
);

public record RespostaDetalhadaDto(
    int PerguntaId,
    string TextoPergunta,
    int OpcaoSelecionadaId,
    string TextoOpcaoSelecionada,
    bool IsCorrecta,
    string? Explicacao
);
