namespace EconomiaComHistoria.Core.Entities;

public class RespostaForum : Comentario
{
    public int ComentarioId { get; set; }
    public Comentario? Comentario { get; set; }
}
