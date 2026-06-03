using EconomiaComHistoria.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace EconomiaComHistoria.Core.Entities;

public class RelatorioProgresso
{
    [Key] public int Id { get; set; }
    public string Periodo { get; set; } = string.Empty;
    public DateTime DataGeracao { get; set; } = DateTime.UtcNow;
    public EstadoRelatorio Estado { get; set; } = EstadoRelatorio.Processando;
    public string? UrlDownload { get; set; }
    public int TotalUtilizadores { get; set; }
    public float MediaPontos { get; set; }
    public float MediaQuizzes { get; set; }
    public float TaxaAcertoGeral { get; set; }

    public int? EscolaId { get; set; }
    public Escola? Escola { get; set; }
    public int? TurmaId { get; set; }
    public Turma? Turma { get; set; }
    public int SolicitadoPorId { get; set; }
    public Utilizador SolicitadoPor { get; set; } = null!;
}
