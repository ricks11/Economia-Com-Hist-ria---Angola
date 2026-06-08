using EconomiaComHistoria.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace EconomiaComHistoria.Core.Entities;

public class RelatorioProgresso
{
    [Key] public int Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty; // PDF, CSV
    public DateTime DataSolicitacao { get; set; } = DateTime.UtcNow;
    public DateTime? DataConclusao { get; set; }
    public EstadoRelatorio Estado { get; set; } = EstadoRelatorio.Pendente;
    public string? UrlDownload { get; set; }

    public int UtilizadorId { get; set; }
    public Utilizador Utilizador { get; set; } = null!;
    public int? EscolaId { get; set; }
    public Escola? Escola { get; set; }
    public int? TurmaId { get; set; }
    public Turma? Turma { get; set; }
}
