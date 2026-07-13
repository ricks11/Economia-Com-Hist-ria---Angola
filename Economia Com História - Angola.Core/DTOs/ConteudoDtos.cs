using EconomiaComHistoria.Core.Enums;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace EconomiaComHistoria.Core.DTOs;

public record ConteudoResponseDto(
    int Id,
    string Titulo,
    string? Resumo,
    string? CorpoTexto,
    string? VideoUrl,
    string? AudioUrl,
    string? ThumbnailUrl,

    [property: JsonConverter(typeof(JsonStringEnumConverter))] TipoConteudo Tipo,
    [property: JsonConverter(typeof(JsonStringEnumConverter))] NivelDificuldade Nivel,

    string Tema,
    string Regiao,

    [property: JsonConverter(typeof(JsonStringEnumConverter))] EstadoConteudo Estado,

    int? EditorId,
    string? EditorNome,
    int Visualizacoes,
    bool EhFavorito,
    bool IsJindungo,
    string? ReferenciaFactual,
    DateTime? DataPublicacao
);

public class CreateConteudoDto : IValidatableObject
{
    [Required(ErrorMessage = "Título é obrigatório")]
    [StringLength(200, MinimumLength = 5, ErrorMessage = "Título deve ter entre 5 e 200 caracteres")]
    public string Titulo { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Resumo não pode exceder 500 caracteres")]
    public string? Resumo { get; set; }

    [StringLength(5000, ErrorMessage = "Corpo do texto não pode exceder 5000 caracteres")]
    public string? CorpoTexto { get; set; }

    [Url(ErrorMessage = "VideoUrl deve ser um URL válido")]
    public string? VideoUrl { get; set; }

    [Url(ErrorMessage = "AudioUrl deve ser um URL válido")]
    public string? AudioUrl { get; set; }

    [Url(ErrorMessage = "ThumbnailUrl deve ser um URL válido")]
    public string? ThumbnailUrl { get; set; }

    [Required(ErrorMessage = "Tema é obrigatório")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Tema deve ter entre 3 e 100 caracteres")]
    public string Tema { get; set; } = string.Empty;

    [Required(ErrorMessage = "Nível de dificuldade é obrigatório")]
    public NivelDificuldade Nivel { get; set; }

    [Required(ErrorMessage = "Região é obrigatória")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Região deve ter entre 3 e 100 caracteres")]
    public string Regiao { get; set; } = string.Empty;

    [Required(ErrorMessage = "Tipo de conteúdo é obrigatório")]
    public TipoConteudo Tipo { get; set; }

    [Required(ErrorMessage = "Estado é obrigatório")]
    public EstadoConteudo Estado { get; set; }

    public bool IsJindungo { get; set; }

    [StringLength(500, ErrorMessage = "Referência Factual não pode exceder 500 caracteres")]
    public string? ReferenciaFactual { get; set; }

    [DataType(DataType.Date)]
    public DateTime? DataAgendada { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (DataAgendada.HasValue && Estado == EstadoConteudo.Publicado)
        {
            yield return new ValidationResult(
                "Não podes definir uma Data de Agendamento com o Estado já em 'Publicado'. Escolhe 'Rascunho' ou 'Em Revisão' para agendar.",
                new[] { nameof(Estado), nameof(DataAgendada) });
        }

        if (DataAgendada.HasValue && DataAgendada.Value <= DateTime.UtcNow)
        {
            yield return new ValidationResult(
                "A Data de Agendamento tem de ser no futuro.",
                new[] { nameof(DataAgendada) });
        }
    }
}

public class UpdateConteudoDto : IValidatableObject
{
    [StringLength(200, MinimumLength = 5, ErrorMessage = "Título deve ter entre 5 e 200 caracteres")]
    public string? Titulo { get; set; }

    [StringLength(500, ErrorMessage = "Resumo não pode exceder 500 caracteres")]
    public string? Resumo { get; set; }

    [StringLength(5000, ErrorMessage = "Corpo do texto não pode exceder 5000 caracteres")]
    public string? CorpoTexto { get; set; }

    [Url(ErrorMessage = "VideoUrl deve ser um URL válido")]
    public string? VideoUrl { get; set; }

    [Url(ErrorMessage = "AudioUrl deve ser um URL válido")]
    public string? AudioUrl { get; set; }

    [Url(ErrorMessage = "ThumbnailUrl deve ser um URL válido")]
    public string? ThumbnailUrl { get; set; }

    [StringLength(100, MinimumLength = 3, ErrorMessage = "Tema deve ter entre 3 e 100 caracteres")]
    public string? Tema { get; set; }

    public NivelDificuldade? Nivel { get; set; }

    [StringLength(100, MinimumLength = 3, ErrorMessage = "Região deve ter entre 3 e 100 caracteres")]
    public string? Regiao { get; set; }

    public TipoConteudo? Tipo { get; set; }

    public EstadoConteudo? Estado { get; set; }

    public bool? IsJindungo { get; set; }

    [StringLength(500, ErrorMessage = "Referência Factual não pode exceder 500 caracteres")]
    public string? ReferenciaFactual { get; set; }

    [DataType(DataType.Date)]
    public DateTime? DataAgendada { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (DataAgendada.HasValue && Estado == EstadoConteudo.Publicado)
        {
            yield return new ValidationResult(
                "Não podes definir uma Data de Agendamento com o Estado já em 'Publicado'.",
                new[] { nameof(Estado), nameof(DataAgendada) });
        }
    }
}

public class ToggleFavoritoResponseDto
{
    public bool Adicionado { get; set; }
    public string Message { get; set; } = string.Empty;
}
