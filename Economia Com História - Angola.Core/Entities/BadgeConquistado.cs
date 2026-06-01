namespace EconomiaComHistoria.Core.Entities;

public class BadgeConquistado
{
    public int Id { get; set; }
    public int UtilizadorId { get; set; }
    public Utilizador? Utilizador { get; set; }
    public int BadgeId { get; set; }
    public Badge? Badge { get; set; }
    public DateTime DataConquista { get; set; }
}
