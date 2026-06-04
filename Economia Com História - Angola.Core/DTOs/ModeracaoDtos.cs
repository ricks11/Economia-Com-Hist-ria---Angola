using EconomiaComHistoria.Core.Enums;

namespace EconomiaComHistoria.Core.DTOs;

public record RejeitarTopicoDto(string MotivoRejeicao);

public record SuspenderUtilizadorDto(int? DiasSuspensao, string? Motivo);

public record ModeracaoPendenteDto(
    int Id,
    string Tipo, // "Topico" ou "Resposta"
    string TituloOuConteudo,
    int AutorId,
    string? AutorNome,
    DateTime DataCriacao,
    int? CategoriaId,
    string? CategoriaNome,
    int? TopicoId,
    int TotalDenuncias);

public record ModeracaoPendentesResponse(
    List<ModeracaoPendenteDto> Topicos,
    List<ModeracaoPendenteDto> Respostas);

public record DenunciaSummaryDto(
    int Id,
    string Tipo, // "Topico" ou "Resposta"
    string TituloOuConteudo,
    int AutorId,
    string? AutorNome,
    int TotalDenuncias,
    DateTime UltimaDenuncia);

public record UtilizadorModeracaoDto(
    int Id,
    string Nome,
    string Email,
    string? Tipo,
    bool Suspenso,
    DateTime? SuspensoAte,
    bool SuspensaoPermanente);
