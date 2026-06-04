using System.ComponentModel.DataAnnotations;

namespace EconomiaComHistoria.Core.Entities;

public class RespostaPergunta
{
    [Key] public int Id { get; set; }
    public int OpcaoRespostaId { get; set; }
    public OpcaoResposta OpcaoResposta { get; set; } = null!;
    public bool Correta { get; set; }
    public int TempoRespostaSeg { get; set; }

    public int TentativaId { get; set; }
    public TentativaQuiz Tentativa { get; set; } = null!;
    public int PerguntaId { get; set; }
    public Pergunta Pergunta { get; set; } = null!;
}
