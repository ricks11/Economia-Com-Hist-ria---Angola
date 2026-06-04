using System.ComponentModel.DataAnnotations;

namespace EconomiaComHistoria.Core.Entities;

public class ConteudoFavorito
{
    [Key] public int Id { get; set; }
    public int ConteudoId { get; set; }
    public Conteudo? Conteudo { get; set; }
    public int UtilizadorId { get; set; }
    public Utilizador? Utilizador { get; set; }
    public DateTime DataAdicionado { get; set; } = DateTime.UtcNow;
}
