using EconomiaComHistoria.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace EconomiaComHistoria.Core.Entities;

public class Reacao
{
    [Key] public int Id { get; set; }
    /// <summary>Emoji da reacção: 📚 = aprendi, 🤔 = quero saber mais, 💬 = debater</summary>
    public string Emoji { get; set; } = string.Empty;
    public DateTime CriadaEm { get; set; } = DateTime.UtcNow;

    public int UtilizadorId { get; set; }
    public Utilizador Utilizador { get; set; } = null!;
    public int? TopicoForumId { get; set; }
    public TopicoForum? TopicoForum { get; set; }
    public int? ComentarioId { get; set; }
    public Comentario? Comentario { get; set; }
}
