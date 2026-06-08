using System.ComponentModel.DataAnnotations;

namespace EconomiaComHistoria.Core.Entities;

public class PlanoEstudo
{
    [Key] public int Id { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string Titulo { get; set; } = null!;
    
    public string? Descricao { get; set; }
    
    public DateTime DataInicio { get; set; } = DateTime.UtcNow;
    
    public DateTime? DataFim { get; set; }

    public int UtilizadorId { get; set; }
    public Utilizador Utilizador { get; set; } = null!;

    // Conteúdos sugeridos para os temas com lacunas
    public ICollection<Conteudo> ConteudosSugeridos { get; set; } = new List<Conteudo>();
}
