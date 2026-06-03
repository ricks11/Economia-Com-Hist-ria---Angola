using EconomiaComHistoria.Core.Enums;

namespace EconomiaComHistoria.Core.Entities;

public class RelatorioProgresso
{
    public int Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string? Resumo { get; set; }
    public string? Tipo { get; set; } // PDF, CSV
    public string? CaminhoFicheiro { get; set; }
    public EstadoRelatorio Estado { get; set; }
    public DateTime DataSolicitacao { get; set; } = DateTime.UtcNow;
    public DateTime? DataConclusao { get; set; }
    public int? UtilizadorId { get; set; }
    public Utilizador? Utilizador { get; set; }
    public int? TurmaId { get; set; }
    public Turma? Turma { get; set; }
    public int? EscolaId { get; set; }
    public Escola? Escola { get; set; }
}
