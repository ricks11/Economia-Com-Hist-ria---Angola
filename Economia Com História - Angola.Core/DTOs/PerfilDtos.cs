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

public record UpdatePerfilDto(string? Nome, string? Provincia, int? EscolaId, int? TurmaId);

public record PerfilDto(
    int Id,
    string Nome,
    string Email,
    string Tipo
);

public record UpdatePerfilDtos(string Nome);