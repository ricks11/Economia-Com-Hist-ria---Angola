using EconomiaComHistoria.Core.Enums;

namespace ECHA.API.DTOs;

public record ConteudoResponseDto(int Id, string Titulo, string? Resumo, string? CorpoTexto,
    string? VideoUrl, string? AudioUrl, string? ThumbnailUrl, TipoConteudo Tipo,
    NivelDificuldade Nivel, string Tema, string Regiao, EstadoConteudo Estado,
    int? EditorId, string? EditorNome, int Visualizacoes, bool EhFavorito,
    bool IsJindungo, string? ReferenciaFactual, DateTime? DataPublicacao);
public record CreateConteudoDto(string Titulo, string? Resumo, string? CorpoTexto,
    string? VideoUrl, string? AudioUrl, string? ThumbnailUrl, string Tema,
    NivelDificuldade Nivel, string Regiao, TipoConteudo Tipo,
    bool IsJindungo = false, string? ReferenciaFactual = null);
public record UpdateConteudoDto(string? Titulo, string? Resumo, string? CorpoTexto,
    string? VideoUrl, string? AudioUrl, string? ThumbnailUrl, string? Tema,
    NivelDificuldade? Nivel, string? Regiao, TipoConteudo? Tipo,
    bool? IsJindungo, string? ReferenciaFactual);
