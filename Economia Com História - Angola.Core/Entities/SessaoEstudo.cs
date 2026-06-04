using System.ComponentModel.DataAnnotations;

namespace EconomiaComHistoria.Core.Entities;

public class SessaoEstudo
{
    [Key] public int Id { get; set; }
    public DateTime Inicio { get; set; } = DateTime.UtcNow;
    public DateTime? Fim { get; set; }
    public int PosicaoPausa { get; set; }       // segundos para vídeo/podcast
    public float PercentagemVista { get; set; }

    public int UtilizadorId { get; set; }
    public Utilizador Utilizador { get; set; } = null!;
    public int ConteudoId { get; set; }
    public Conteudo Conteudo { get; set; } = null!;
}
