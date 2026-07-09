using EconomiaComHistoria.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace EconomiaComHistoria.Core.DTOs;

public record PerfilResponseDto(
    int Id, 
    string Nome, 
    string Email, 
    string? Telemovel,
    TipoUtilizador Tipo, 
    DateTime DataRegisto, 
    int PontosTotais, 
    int StreakAtual,
    string? Provincia, 
    int? EscolaId, 
    string? EscolaNome, 
    int? TurmaId, 
    string? TurmaNome);

public class UpdatePerfilDto
{
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Nome deve ter entre 3 e 100 caracteres")]
    public string? Nome { get; set; }

    [StringLength(100, MinimumLength = 2, ErrorMessage = "Província deve ter entre 2 e 100 caracteres")]
    public string? Provincia { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Escola ID deve ser válido")]
    public int? EscolaId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Turma ID deve ser válido")]
    public int? TurmaId { get; set; }
}

public record PerfilDto(
    int Id,
    string Nome,
    string Email,
    string Tipo
);

public record UpdatePerfilDtos(string Nome);