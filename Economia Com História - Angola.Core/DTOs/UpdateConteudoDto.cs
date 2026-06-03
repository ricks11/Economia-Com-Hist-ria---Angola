namespace EconomiaComHistoria.Core.DTOs;

public record UpdateConteudoDto(
    string? Titulo,
    string? Resumo,
    string? Texto,
    string? Tema,
    string? Nivel,
    string? Regiao,
    string? Tipo,
    string? UrlMedia);
