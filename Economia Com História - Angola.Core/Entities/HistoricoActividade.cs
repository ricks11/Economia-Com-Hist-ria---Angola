namespace EconomiaComHistoria.Core.Entities;

public class HistoricoActividade
{
    public int Id { get; set; }
    public int UtilizadorId { get; set; }
    public Utilizador? Utilizador { get; set; }
    public string Accao { get; set; } = string.Empty;
    public string? Detalhes { get; set; }
    public DateTime DataActividade { get; set; }
}
