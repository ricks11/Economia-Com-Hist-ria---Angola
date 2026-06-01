namespace EconomiaComHistoria.Core.Entities;

public class Turma
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public int EscolaId { get; set; }
    public Escola? Escola { get; set; }
}
