namespace EconomiaComHistoria.Core.Entities;

public class Conteudo
{
    public int Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string? Resumo { get; set; }
    public string? Texto { get; set; }
    public DateTime DataPublicacao { get; set; }
    public int AutorId { get; set; }
    public Utilizador? Autor { get; set; }
    public ICollection<Quiz> Quizzes { get; set; } = new List<Quiz>();
}
