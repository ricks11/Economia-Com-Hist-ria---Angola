namespace EconomiaComHistoria.Core.Entities;

public class Comentario
{
    public int Id { get; set; }
    public string Texto { get; set; } = string.Empty;
    public DateTime DataCriacao { get; set; }
    public int AutorId { get; set; }
    public Utilizador? Autor { get; set; }
    public ICollection<RespostaForum> Respostas { get; set; } = new List<RespostaForum>();
}
