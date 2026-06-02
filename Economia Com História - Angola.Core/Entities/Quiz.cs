namespace EconomiaComHistoria.Core.Entities;

public class Quiz
{
    public int Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public int NivelDificuldade { get; set; }
    public string Tema { get; set; } = string.Empty;
    public int NumeroPerguntas { get; set; }
    public int TempoPorPerguntaSegundos { get; set; }
    public bool IsDeleted { get; set; } = false;
    public ICollection<Pergunta> Perguntas { get; set; } = new List<Pergunta>();
    public ICollection<TentativaQuiz> Tentativas { get; set; } = new List<TentativaQuiz>();
}
