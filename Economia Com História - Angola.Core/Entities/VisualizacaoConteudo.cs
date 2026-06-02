namespace EconomiaComHistoria.Core.Entities;

public class VisualizacaoConteudo
{
    public int Id { get; set; }
    public int ConteudoId { get; set; }
    public Conteudo? Conteudo { get; set; }
    public int UtilizadorId { get; set; }
    public Utilizador? Utilizador { get; set; }
    public DateTime DataVisualizacao { get; set; } = DateTime.UtcNow;
}
