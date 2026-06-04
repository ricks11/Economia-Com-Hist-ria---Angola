namespace ECHA.Mobile.Models;

public record RespostaDto(Guid Id, string Texto, bool IsCorreta);

public record PerguntaDto(Guid Id, string Texto, List<RespostaDto> Respostas, string? Explicacao);

public record QuizDto(Guid Id, string Titulo, string Tema, string Nivel, List<PerguntaDto> Perguntas);
