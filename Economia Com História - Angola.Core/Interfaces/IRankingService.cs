using EconomiaComHistoria.Core.Entities;
using EconomiaComHistoria.Core.Enums;

namespace EconomiaComHistoria.Core.Interfaces;

public interface IRankingService
{
    Task GerarSnapshotSemanalAsync(CancellationToken ct = default);
    Task<List<EntradaRanking>> GetRankingAsync(
        TipoRanking tipo,
        PeriodoRanking periodo,
        int? escolaId = null,
        string? provincia = null,
        string? municipio = null,
        CancellationToken ct = default);
}