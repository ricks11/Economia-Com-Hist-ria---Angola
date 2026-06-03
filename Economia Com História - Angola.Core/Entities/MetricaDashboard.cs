using EconomiaComHistoria.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace EconomiaComHistoria.Core.Entities;

public class MetricaDashboard
{
    [Key] public int Id { get; set; }
    public TipoMetricaDashboard Tipo { get; set; }
    public float Valor { get; set; }
    public string? Dimensao { get; set; }           // ex: "provincia"
    public string? ValorDimensao { get; set; }      // ex: "Luanda"
    public DateTime Data { get; set; }
    public DateTime CalculadaEm { get; set; } = DateTime.UtcNow;
}
