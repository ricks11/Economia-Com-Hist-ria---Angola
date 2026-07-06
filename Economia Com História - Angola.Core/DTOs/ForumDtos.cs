using EconomiaComHistoria.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace EconomiaComHistoria.Core.DTOs;

public class CriarTopicoForumDto
{
    [Required(ErrorMessage = "Título é obrigatório")]
    [StringLength(200, MinimumLength = 5, ErrorMessage = "Título deve ter entre 5 e 200 caracteres")]
    public string Titulo { get; set; } = string.Empty;

    [Required(ErrorMessage = "Descrição é obrigatória")]
    [StringLength(2000, MinimumLength = 10, ErrorMessage = "Descrição deve ter entre 10 e 2000 caracteres")]
    public string Descricao { get; set; } = string.Empty;

    [Required(ErrorMessage = "Categoria é obrigatória")]
    [Range(1, int.MaxValue, ErrorMessage = "Categoria ID deve ser válido")]
    public int CategoriaId { get; set; }
}

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

public class CriarRespostaForumDto
{
    [Required(ErrorMessage = "Conteúdo é obrigatório")]
    [StringLength(2000, MinimumLength = 5, ErrorMessage = "Conteúdo deve ter entre 5 e 2000 caracteres")]
    public string Conteudo { get; set; } = string.Empty;

    [Range(0, int.MaxValue, ErrorMessage = "Resposta Pai ID deve ser válido")]
    public int? RespostaPaiId { get; set; }
}

public class AtualizarRespostaForumDto
{
    [Required(ErrorMessage = "Conteúdo é obrigatório")]
    [StringLength(2000, MinimumLength = 5, ErrorMessage = "Conteúdo deve ter entre 5 e 2000 caracteres")]
    public string Conteudo { get; set; } = string.Empty;
}

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

public class CriarReacaoDto : IValidatableObject
{
    [Range(1, int.MaxValue, ErrorMessage = "Tópico ID deve ser maior que zero")]
    public int? TopicoForumId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Resposta ID deve ser maior que zero")]
    public int? RespostaForumId { get; set; }

    [Required(ErrorMessage = "Emoji é obrigatório")]
    [StringLength(10, MinimumLength = 1, ErrorMessage = "Emoji deve ter entre 1 e 10 caracteres")]
    public string Emoji { get; set; } = string.Empty;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!TopicoForumId.HasValue && !RespostaForumId.HasValue)
        {
            yield return new ValidationResult(
                "É necessário especificar pelo menos um dos IDs: TopicoForumId ou RespostaForumId.",
                new[] { nameof(TopicoForumId), nameof(RespostaForumId) });
        }
        else if (TopicoForumId.HasValue && RespostaForumId.HasValue)
        {
            yield return new ValidationResult(
                "Apenas um dos IDs deve ser especificado: TopicoForumId ou RespostaForumId.",
                new[] { nameof(TopicoForumId), nameof(RespostaForumId) });
        }
    }
}

public record CriarDenunciaDto(int? TopicoForumId, int? RespostaForumId, MotivoDenuncia Motivo, string? Descricao);

public record RejeitarTopicoDto(string MotivoRejeicao);

public record SuspenderUtilizadorDto(int? DiasSuspensao);
