namespace EconomiaComHistoria.Core.Entities;

public class DecisaoModeracao
{
    public int Id { get; set; }
    public string Decisao { get; set; } = string.Empty;
    public string? Observacoes { get; set; }
    public DateTime DataDecisao { get; set; }
    public int ModeracaoId { get; set; }
    public Moderacao? Moderacao { get; set; }
}
