using EconomiaComHistoria.Core.Entities;
using EconomiaComHistoria.Core.Enums;

namespace EconomiaComHistoria.Core.Interfaces;

public interface IRankingService
{
    /// <summary>
    /// Generates a snapshot of current rankings for the week.
    /// </summary>
    Task GerarSnapshotSemanalAsync();

    /// <summary>
    /// Retrieves the ranking based on type, period and optional filters.
    /// </summary>
    /// <param name="tipo">general, escola, or provincia</param>
    /// <param name="periodo">Semanal or Historico</param>
    /// <param name="escolaId">Optional filter for school</param>
    /// <param name="provincia">Optional filter for province</param>
    Task<List<Ranking>> GetRankingAsync(string tipo, PeriodoRanking periodo, int? escolaId = null, string? provincia = null);
}
