namespace EconomiaComHistoria.Core.DTOs;

public record CreateEscolaDto(string Nome, string? CodigoMEC, string? Provincia, string? Localizacao);

public record EscolaResponseDto(
    int Id,
    string Nome,
    string? CodigoMEC,
    string? Provincia,
    string? Localizacao,
    string? CodigoConvite,
    DateTime? ConviteExpiraEm,
    int TotalAlunos,
    int TotalTurmas);

public record InviteCodeResponseDto(string Codigo, DateTime ExpiraEm);

public record AssociarAlunoDto(string Codigo);
