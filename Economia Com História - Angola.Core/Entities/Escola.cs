namespace EconomiaComHistoria.Core.Entities;

public class Escola
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Localizacao { get; set; }
    public ICollection<Turma> Turmas { get; set; } = new List<Turma>();
    public ICollection<Utilizador> Utilizadores { get; set; } = new List<Utilizador>();
}
