using System.ComponentModel.DataAnnotations;

namespace EconomiaComHistoria.Core.Entities;

public class PlanoEstudo
{
    [Key] public int Id { get; set; }
    /// <summary>JSON serializado: ["Mercado de Capitais","Política Fiscal"]</summary>
    public string TemasLacunaSerialized { get; set; } = "[]";
    public DateTime DataGeracao { get; set; } = DateTime.UtcNow;
    public DateTime DataExpiracao { get; set; }

    public int UtilizadorId { get; set; }
    public Utilizador Utilizador { get; set; } = null!;

    // Conteúdos sugeridos para os temas com lacunas
    public ICollection<Conteudo> ConteudosSugeridos { get; set; } = new List<Conteudo>();
}
