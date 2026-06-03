using System.ComponentModel.DataAnnotations;

namespace EconomiaComHistoria.Core.Entities;

public class Badge
{
    [Key] public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public string? IconeUrl { get; set; }
    public string CriterioTipo { get; set; } = string.Empty;   // ex: "quiz_completados"
    public int CriterioValor { get; set; }                     // ex: 1 (primeiro quiz)
    public bool Ativo { get; set; } = true;

    public ICollection<BadgeConquistado> Conquistado { get; set; } = new List<BadgeConquistado>();
}
