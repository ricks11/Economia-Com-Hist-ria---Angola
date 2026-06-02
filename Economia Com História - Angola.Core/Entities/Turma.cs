namespace EconomiaComHistoria.Core.Entities;

public class Turma
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public int? Ano { get; set; }
    public int EscolaId { get; set; }
    public Escola? Escola { get; set; }
    public int? ProfessorId { get; set; }
    public Utilizador? Professor { get; set; }
}
