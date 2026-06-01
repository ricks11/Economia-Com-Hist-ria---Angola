namespace EconomiaComHistoria.Core.Entities;

public class TopicoForum
{
    public int Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string? Conteudo { get; set; }
    public DateTime DataCriacao { get; set; }
    public int AutorId { get; set; }
    public Utilizador? Autor { get; set; }
    public ICollection<Comentario> Comentarios { get; set; } = new List<Comentario>();
}
