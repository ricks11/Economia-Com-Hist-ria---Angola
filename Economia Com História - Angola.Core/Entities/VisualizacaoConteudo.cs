using System.ComponentModel.DataAnnotations;

namespace EconomiaComHistoria.Core.Entities;

public class VisualizacaoConteudo
{
    [Key] public int Id { get; set; }
    public DateTime DataHora { get; set; } = DateTime.UtcNow;
    public bool Completa { get; set; }     // visualizou 100%

    public int UtilizadorId { get; set; }
    public Utilizador Utilizador { get; set; } = null!;
    public int ConteudoId { get; set; }
    public Conteudo Conteudo { get; set; } = null!;
}
