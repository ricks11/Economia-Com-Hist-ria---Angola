namespace EconomiaComHistoria.Core.Entities;

public class RelatorioProgresso
{
    public int Id { get; set; }
    public string Resumo { get; set; } = string.Empty;
    public DateTime DataGeracao { get; set; }
    public int UtilizadorId { get; set; }
    public Utilizador? Utilizador { get; set; }
}
