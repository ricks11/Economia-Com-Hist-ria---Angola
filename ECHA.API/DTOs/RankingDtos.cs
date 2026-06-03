namespace EconomiaComHistoria.API.DTOs;

public record RankingEntradaDto(int Posicao, int UtilizadorId, string NomeUtilizador,
    int Pontos, int QuizzesCompletados, string? EscolaNome);