using EconomiaComHistoria.Core.Entities.Quiz;
using EconomiaComHistoria.Core.Enums;

namespace EconomiaComHistoria.Core.Interfaces;

public interface IRankingService
{
    Task GerarSnapshotSemanalAsync();
    Task<List<EntradaRanking>> GetRankingAsync(string tipo, PeriodoRanking periodo, int? escolaId = null, string? provincia = null);
}