using EconomiaComHistoria.Core.Enums;

namespace EconomiaComHistoria.Core.Entities;

public class Ranking
{
    public int Id { get; set; }
    public int UtilizadorId { get; set; }
    public Utilizador? Utilizador { get; set; }
    public int Pontuacao { get; set; }
    public PeriodoRanking Periodo { get; set; }
    public int? EscolaId { get; set; }
    public string? Provincia { get; set; }
    public DateTime DataSnapshot { get; set; }
}
