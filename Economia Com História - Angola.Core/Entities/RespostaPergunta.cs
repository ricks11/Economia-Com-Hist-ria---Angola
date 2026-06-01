namespace EconomiaComHistoria.Core.Entities;

public class RespostaPergunta
{
    public int Id { get; set; }
    public int PerguntaId { get; set; }
    public Pergunta? Pergunta { get; set; }
    public int TentativaQuizId { get; set; }
    public TentativaQuiz? TentativaQuiz { get; set; }
    public string? TextoResposta { get; set; }
    public bool Correta { get; set; }
}
