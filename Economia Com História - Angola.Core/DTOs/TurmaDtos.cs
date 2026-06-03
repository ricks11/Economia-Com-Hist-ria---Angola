namespace EconomiaComHistoria.Core.DTOs;

public record CreateTurmaDto(string Nome, int? Ano, int EscolaId, int? ProfessorId);

public record UpdateTurmaDto(string Nome, int? Ano, int? ProfessorId);

public record TurmaResponseDto(
    int Id,
    string Nome,
    int? Ano,
    int EscolaId,
    string? EscolaNome,
    int? ProfessorId,
    string? ProfessorNome,
    int TotalAlunos);

public record TurmaDetalheDto(
    int Id,
    string Nome,
    int? Ano,
    int EscolaId,
    string? EscolaNome,
    int? ProfessorId,
    string? ProfessorNome,
    List<AlunoResumoDto> Alunos);

public record AlunoResumoDto(int Id, string Nome, string Email, int PontosTotais);
