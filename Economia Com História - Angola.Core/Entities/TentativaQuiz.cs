using System.ComponentModel.DataAnnotations;

namespace EconomiaComHistoria.Core.Entities;

public class TentativaQuiz
{
    [Key] public int Id { get; set; }
    public DateTime DataHora { get; set; } = DateTime.UtcNow;
    public DateTime DataFim { get; set; }
    public int Pontuacao { get; set; }
    public int BonusVelocidade { get; set; }
    public int TempoGastoSeg { get; set; }
    public bool Completada { get; set; }
    public int TotalPerguntas { get; set; }
    public int TotalCorretas { get; set; }
    public bool SincronizadaOffline { get; set; }   // veio de sync offline
    public DateTime? TimestampCliente { get; set; } // para validação anti-fraude

    public int UtilizadorId { get; set; }
    public Utilizador Utilizador { get; set; } = null!;
    public int QuizId { get; set; }
    public Quiz Quiz { get; set; } = null!;

    public ICollection<RespostaPergunta> Respostas { get; set; } = new List<RespostaPergunta>();
}
