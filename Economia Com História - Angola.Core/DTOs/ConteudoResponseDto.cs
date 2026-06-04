using EconomiaComHistoria.Core.Enums;

namespace EconomiaComHistoria.Core.DTOs;

public record ConteudoResponseDto(
    int Id,
    string Titulo,
    string? Resumo,
    string? Texto,
    DateTime DataPublicacao,
    int AutorId,
    string? AutorNome,
    string? Tema,
    string? Nivel,
    string? Regiao,
    string? Tipo,
    EconomiaComHistoria.Core.Enums.EstadoConteudo Estado,
    string? ImagemCapa,
    string? UrlMedia,
    bool IsJindungo,
    string? ReferenciaFactual,
    int Visualizacoes,
    bool EhFavorito);
