using System.ComponentModel.DataAnnotations;

namespace EconomiaComHistoria.Core.Entities;

public class ConteudoTraducao
{
    [Key] public int Id { get; set; }
    public string Lingua { get; set; } = string.Empty;   // ex: "Kimbundu", "Umbundu"
    public string? TextoTraduzido { get; set; }
    public string? AudioUrl { get; set; }

    public int ConteudoId { get; set; }
    public Conteudo Conteudo { get; set; } = null!;
}
