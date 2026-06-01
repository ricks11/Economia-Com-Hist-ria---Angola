namespace EconomiaComHistoria.Core.DTOs;

public class ConteudoDto
{
    public int Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string? Resumo { get; set; }
    public string? Texto { get; set; }
}
