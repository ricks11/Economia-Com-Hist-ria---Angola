using System.ComponentModel.DataAnnotations;
using EconomiaComHistoria.Core.Enums;

namespace EconomiaComHistoria.Core.Entities;

public class RelatorioProgresso
{
    [Key]
    public int Id { get; set; }

    public string Titulo { get; set; } = string.Empty;

    public string Tipo { get; set; } = "PDF"; // PDF ou CSV

    public EstadoRelatorio Estado { get; set; } = EstadoRelatorio.Pendente;

    public DateTime DataSolicitacao { get; set; }

    public DateTime? DataConclusao { get; set; }

    // ✅ Usar UrlDownload (como está na tabela) em vez de CaminhoArquivo
    public string? UrlDownload { get; set; }

    // ❌ Removido: CaminhoArquivo, MensagemErro, ParametrosJson
    // Essas colunas não existem na tabela

    public int UtilizadorId { get; set; }
    public Utilizador Utilizador { get; set; } = null!;

    public int? TurmaId { get; set; }
    public Turma? Turma { get; set; }

    public int? EscolaId { get; set; }
    public Escola? Escola { get; set; }
}