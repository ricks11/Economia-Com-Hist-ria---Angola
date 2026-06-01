namespace EconomiaComHistoria.Core.Entities;

public class Moderacao
{
    public int Id { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime DataCriacao { get; set; }
    public int? ConteudoId { get; set; }
    public Conteudo? Conteudo { get; set; }
    public int? ComentarioId { get; set; }
    public Comentario? Comentario { get; set; }
    public ICollection<DecisaoModeracao> Decisoes { get; set; } = new List<DecisaoModeracao>();
}
