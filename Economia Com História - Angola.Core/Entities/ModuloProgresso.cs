using System.ComponentModel.DataAnnotations;

namespace EconomiaComHistoria.Core.Entities;

public class ModuloProgresso
{
    [Key] public int Id { get; set; }
    public string Tema { get; set; } = string.Empty;
    public int MinutosAssistidos { get; set; }
    public int QuizzesCompletados { get; set; }
    public int TopicosCriados { get; set; }
    public int ComentariosPublicados { get; set; }
    public float MediaAcerto { get; set; }     // 0.0 a 1.0 — usado no algoritmo de sugestão

    public int UtilizadorId { get; set; }
    public Utilizador Utilizador { get; set; } = null!;
}
