using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EconomiaComHistoria.Core.Entities;

public class SolicitacaoAcesso
{
    [Key] public int Id { get; set; }
    public int UtilizadorId { get; set; }
    [ForeignKey(nameof(UtilizadorId))]
    public Utilizador Utilizador { get; set; } = null!;
    public int ConteudoId { get; set; }
    [ForeignKey(nameof(ConteudoId))]
    public Conteudo Conteudo { get; set; } = null!;
    public DateTime DataSolicitacao { get; set; } = DateTime.UtcNow;
    public string Status { get; set; } = "Pendente"; // Pendente, Aprovado, Rejeitado
    public DateTime? DataDecisao { get; set; }
    public int? ModeradorId { get; set; }
}