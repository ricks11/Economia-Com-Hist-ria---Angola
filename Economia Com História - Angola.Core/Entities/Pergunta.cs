namespace EconomiaComHistoria.Core.Entities;

public class Pergunta
{
    public int Id { get; set; }
    public int QuizId { get; set; }
    public Quiz? Quiz { get; set; }
    public string Texto { get; set; } = string.Empty;
    public int OrdemAleatorizada { get; set; }
    public int TempoLimiteSegundos { get; set; }
    public ICollection<OpcaoResposta> Opcoes { get; set; } = new List<OpcaoResposta>();
    public ICollection<RespostaPergunta> Respostas { get; set; } = new List<RespostaPergunta>();
}
