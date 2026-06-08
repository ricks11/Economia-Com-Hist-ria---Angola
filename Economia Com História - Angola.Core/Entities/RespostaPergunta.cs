using System.ComponentModel.DataAnnotations;

namespace EconomiaComHistoria.Core.Entities;

public class RespostaPergunta
{
    [Key] public int Id { get; set; }
    public int OpcaoRespostaId { get; set; }
    public OpcaoResposta OpcaoResposta { get; set; } = null!;
    public bool IsCorrecta { get; set; }
    public int TempoRespostaMs { get; set; }

    public int TentativaQuizId { get; set; }
    public TentativaQuiz TentativaQuiz { get; set; } = null!;
    public int PerguntaId { get; set; }
    public Pergunta Pergunta { get; set; } = null!;
}
