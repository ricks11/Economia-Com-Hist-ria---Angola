namespace EconomiaComHistoria.Core.Entities;

public class ConteudoTraducao
{
    public int Id { get; set; }
    public int ConteudoId { get; set; }
    public Conteudo? Conteudo { get; set; }
    public string Lingua { get; set; } = string.Empty; // Kimbundu, Umbundu, English, etc.
    public string? Texto { get; set; }
    public string? AudioUrl { get; set; }
    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
}
