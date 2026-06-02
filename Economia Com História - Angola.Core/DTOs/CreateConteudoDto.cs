namespace EconomiaComHistoria.Core.DTOs;

public record CreateConteudoDto(
    string Titulo,
    string? Resumo,
    string? Texto,
    string? Tema,
    string? Nivel,
    string? Regiao,
    string? Tipo);
