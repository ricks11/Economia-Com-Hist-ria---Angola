namespace EconomiaComHistoria.Core.Entities;

public class Quiz
{
    public int Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public int ConteudoId { get; set; }
    public Conteudo? Conteudo { get; set; }
    public ICollection<Pergunta> Perguntas { get; set; } = new List<Pergunta>();
    public ICollection<TentativaQuiz> Tentativas { get; set; } = new List<TentativaQuiz>();
}
