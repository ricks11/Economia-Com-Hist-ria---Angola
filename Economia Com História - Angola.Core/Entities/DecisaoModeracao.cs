using EconomiaComHistoria.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace EconomiaComHistoria.Core.Entities;

public class DecisaoModeracao
{
    [Key] public int Id { get; set; }
    public AcaoModeracao Acao { get; set; }
    public string Justificativa { get; set; } = string.Empty;
    public int? DuracaoSuspensaoDias { get; set; }   // preenchido em SuspensoTemp
    public DateTime DataDecisao { get; set; } = DateTime.UtcNow;
    public bool RequereAprovacaoAdmin { get; set; }  // menores de 18 → true (RN-W13)

    public int ModeracaoId { get; set; }
    public Moderacao Moderacao { get; set; } = null!;
    public int ModeradorId { get; set; }
    public Utilizador Moderador { get; set; } = null!;
}
