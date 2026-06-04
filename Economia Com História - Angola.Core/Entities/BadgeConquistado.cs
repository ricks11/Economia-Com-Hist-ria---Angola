using System.ComponentModel.DataAnnotations;

namespace EconomiaComHistoria.Core.Entities;

public class BadgeConquistado
{
    [Key] public int Id { get; set; }
    public DateTime ConquistadoEm { get; set; } = DateTime.UtcNow;

    public int UtilizadorId { get; set; }
    public Utilizador Utilizador { get; set; } = null!;
    public int BadgeId { get; set; }
    public Badge Badge { get; set; } = null!;
}
