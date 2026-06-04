using EconomiaComHistoria.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace EconomiaComHistoria.Core.Entities;

public class Ranking
{
    [Key] public int Id { get; set; }
    public TipoRanking Tipo { get; set; }
    public PeriodoRanking Periodo { get; set; }
    public DateTime DataCalculo { get; set; } = DateTime.UtcNow;
    public string? FiltroId { get; set; }   // id da escola/município/província se aplicável

    public ICollection<EntradaRanking> Entradas { get; set; } = new List<EntradaRanking>();
}
