using System.ComponentModel.DataAnnotations;
using EconomiaComHistoria.Core.Enums;

namespace EconomiaComHistoria.Core.Entities;

public class Escola
{
    [Key]
    public int Id { get; set; }

    [Required, MaxLength(200)]
    public string Nome { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? CodigoMEC { get; set; }  // <-- Adicionado

    public string Provincia { get; set; } = string.Empty;
    public string? Municipio { get; set; }   // Mapeia para Localizacao

    public string? CodigoConvite { get; set; }
    public DateTime? CodigoConviteExpiracao { get; set; }

    public bool Ativa { get; set; } = true;
    public DateTime CriadaEm { get; set; } = DateTime.UtcNow;
    public PlanoContrato PlanoContrato { get; set; } = PlanoContrato.Gratuito;

    // Relacionamentos
    public ICollection<Utilizador> Alunos { get; set; } = new List<Utilizador>();
    public ICollection<Turma> Turmas { get; set; } = new List<Turma>();
    public ICollection<RelatorioProgresso> Relatorios { get; set; } = new List<RelatorioProgresso>();
    public ICollection<EntradaRanking> EntradasRanking { get; set; } = new List<EntradaRanking>();
}