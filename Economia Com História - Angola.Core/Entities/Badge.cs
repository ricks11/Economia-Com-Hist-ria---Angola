namespace EconomiaComHistoria.Core.Entities;

public class Badge
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public ICollection<BadgeConquistado> BadgesConquistados { get; set; } = new List<BadgeConquistado>();
}
