using System.ComponentModel.DataAnnotations;

namespace EconomiaComHistoria.Core.Entities;

public class Turma
{
    [Key] public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Ano { get; set; } = string.Empty;
    public string? Turno { get; set; }
    public bool Ativa { get; set; } = true;

    public int EscolaId { get; set; }
    public Escola Escola { get; set; } = null!;

    // Professor: utilizador com role Professor
    public int? ProfessorId { get; set; }
    public Utilizador? Professor { get; set; }

    public ICollection<Utilizador> Alunos { get; set; } = new List<Utilizador>();
    public ICollection<RelatorioProgresso> Relatorios { get; set; } = new List<RelatorioProgresso>();
}
