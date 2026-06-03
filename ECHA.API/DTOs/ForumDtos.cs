using EconomiaComHistoria.Core.Enums;

namespace EconomiaComHistoria.API.DTOs;

public record CriarTopicoForumDto(string Titulo, string Descricao, int CategoriaId);

public record TopicoForumDto(
    int Id,
    string Titulo,
    string Descricao,
    int CategoriaId,
    string CategoriaNome,
    int AutorId,
    string? AutorNome,
    EstadoTopicoForum Estado,
    DateTime CriadoEm,
    bool Fixado,
    int Visualizacoes);

public record TopicoForumDetalheDto(
    int Id,
    string Titulo,
    string Descricao,
    int CategoriaId,
    string CategoriaNome,
    int AutorId,
    string? AutorNome,
    EstadoTopicoForum Estado,
    DateTime CriadoEm,
    bool Fixado,
    int Visualizacoes,
    IReadOnlyCollection<RespostaForumDto> Respostas);

public record CriarRespostaForumDto(string Conteudo, int? RespostaPaiId);
public record AtualizarRespostaForumDto(string Conteudo);

public record RespostaForumDto(
    int Id,
    string Conteudo,
    int AutorId,
    string? AutorNome,
    EstadoComentario Estado,
    DateTime DataCriacao,
    DateTime? DataEdicao,
    int? RespostaPaiId,
    bool IsSolucao,
    IReadOnlyCollection<RespostaForumDto> RespostasFilhas);

public record CriarReacaoDto(int? TopicoForumId, int? RespostaForumId, string Emoji);
public record CriarDenunciaDto(int? TopicoForumId, int? RespostaForumId, MotivoDenuncia Motivo, string? Descricao);

public record RejeitarTopicoDto(string MotivoRejeicao);

public record SuspenderUtilizadorDto(int? DiasSuspensao);
