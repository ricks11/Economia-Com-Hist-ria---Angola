namespace EconomiaComHistoria.Core.DTOs;

public record RankingEntradaDto(
    int Posicao, 
    int UtilizadorId, 
    string NomeUtilizador,
    int Pontos, 
    int QuizzesCompletados, 
    string? EscolaNome);
