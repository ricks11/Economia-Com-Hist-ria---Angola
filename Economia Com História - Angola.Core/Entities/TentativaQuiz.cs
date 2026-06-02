namespace EconomiaComHistoria.Core.Entities;

public class TentativaQuiz
{
    public int Id { get; set; }
    public int UtilizadorId { get; set; }
    public Utilizador? Utilizador { get; set; }
    public int QuizId { get; set; }
    public Quiz? Quiz { get; set; }
    public DateTime DataInicio { get; set; }
    public DateTime? DataFim { get; set; }
    public int Pontuacao { get; set; }
    public bool Completa { get; set; }
    public ICollection<RespostaPergunta> Respostas { get; set; } = new List<RespostaPergunta>();
}
