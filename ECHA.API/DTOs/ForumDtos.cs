using EconomiaComHistoria.Core.Enums;

namespace EconomiaComHistoria.API.DTOs;

public record CriarTopicoForumDto(string Titulo, string Conteudo, int CategoriaId);

public record TopicoForumDto(
    int Id,
    string Titulo,
    string Conteudo,
    int AutorId,
    string? AutorNome,
    int CategoriaId,
    string? CategoriaNome,
    EstadoTopico EstadoTopico,
    DateTime DataCriacao,
    int TotalDenuncias);

public record TopicoForumDetalheDto(
    int Id,
    string Titulo,
    string Conteudo,
    int AutorId,
    string? AutorNome,
    int CategoriaId,
    string? CategoriaNome,
    EstadoTopico EstadoTopico,
    DateTime DataCriacao,
    int TotalDenuncias,
    IReadOnlyCollection<RespostaForumDto> Respostas);

public record CriarRespostaForumDto(string Conteudo, int? RespostaPaiId);

public record AtualizarRespostaForumDto(string Conteudo);

public record RespostaForumDto(
    int Id,
    int TopicoId,
    int AutorId,
    string? AutorNome,
    string Conteudo,
    int? RespostaPaiId,
    EstadoResposta EstadoResposta,
    DateTime DataCriacao,
    DateTime? DataEdicao,
    IReadOnlyCollection<RespostaForumDto> Respostas);

public record CriarReacaoDto(int? TopicoId, int? RespostaId, TipoReacao TipoReacao);

public record CriarDenunciaDto(int? TopicoId, int? RespostaId, string Motivo);
