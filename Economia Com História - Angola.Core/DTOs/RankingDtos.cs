using System.ComponentModel.DataAnnotations;

namespace EconomiaComHistoria.Core.DTOs;

public record RankingEntradaDto(
    [Range(1, int.MaxValue)] int Posicao, 
    [Range(1, int.MaxValue)] int UtilizadorId, 
    [Required][StringLength(100)] string NomeUtilizador,
    [Range(0, int.MaxValue)] int Pontos, 
    [Range(0, int.MaxValue)] int QuizzesCompletados, 
    [StringLength(100)] string? EscolaNome);
