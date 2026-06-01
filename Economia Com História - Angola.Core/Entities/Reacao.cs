namespace EconomiaComHistoria.Core.Entities;

public class Reacao
{
    public int Id { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public int UtilizadorId { get; set; }
    public Utilizador? Utilizador { get; set; }
    public int? ConteudoId { get; set; }
    public Conteudo? Conteudo { get; set; }
    public int? ComentarioId { get; set; }
    public Comentario? Comentario { get; set; }
}
