namespace EconomiaComHistoria.Core.Entities;

public class ModuloProgresso
{
    public int Id { get; set; }
    public int UtilizadorId { get; set; }
    public Utilizador? Utilizador { get; set; }
    public string Tema { get; set; } = string.Empty;
    public decimal PercentagemCompleta { get; set; } = 0;
    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
    public DateTime? DataUltimaAtualizacao { get; set; }
}
