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
    EstadoConteudo Estado,
    string? ImagemCapa,
    int Visualizacoes,
    bool EhFavorito);
