namespace EconomiaComHistoria.Core.Entities;

public class Escola
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? CodigoMEC { get; set; }
    public string? Provincia { get; set; }
    public string? Localizacao { get; set; }
    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
    public ICollection<Turma> Turmas { get; set; } = new List<Turma>();
    public ICollection<Utilizador> Utilizadores { get; set; } = new List<Utilizador>();
}
