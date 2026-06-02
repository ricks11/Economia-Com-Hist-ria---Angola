namespace EconomiaComHistoria.Core.Entities;

public class RespostaPergunta
{
    public int Id { get; set; }
    public int TentativaQuizId { get; set; }
    public TentativaQuiz? TentativaQuiz { get; set; }
    public int PerguntaId { get; set; }
    public Pergunta? Pergunta { get; set; }
    public int OpcaoRespostaId { get; set; }
    public OpcaoResposta? OpcaoResposta { get; set; }
    public int TempoRespostaMs { get; set; }
    public bool IsCorrecta { get; set; }
}
