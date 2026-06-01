namespace EconomiaComHistoria.Core.Entities;

public class PlanoEstudo
{
    public int Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public DateTime DataInicio { get; set; }
    public DateTime? DataFim { get; set; }
    public int UtilizadorId { get; set; }
    public Utilizador? Utilizador { get; set; }
}
