namespace EconomiaComHistoria.Core.DTOs;

public class RankingResponseDto
{
    public List<RankingEntradaDto> Top100 { get; set; } = new();
    public int PosicaoUtilizador { get; set; }
    public int? PontosUtilizador { get; set; }
    public string? Tipo { get; set; }
    public string? Periodo { get; set; }
    public int UtilizadorId { get; set; }
}
