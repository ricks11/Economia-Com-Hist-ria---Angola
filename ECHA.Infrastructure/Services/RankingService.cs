using EconomiaComHistoria.Core.Entities;
using EconomiaComHistoria.Core.Enums;
using EconomiaComHistoria.Core.Interfaces;
using EconomiaComHistoria.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace EconomiaComHistoria.Infrastructure.Services;

public class RankingService : IRankingService
{
    private readonly AppDbContext _context;
    private readonly IMemoryCache _cache;

    public RankingService(AppDbContext context, IMemoryCache cache)
    {
        _context = context;
        _cache = cache;
    }

    public async Task GerarSnapshotSemanalAsync()
    {
        var now = DateTime.UtcNow;
        var startOfWeek = now.AddDays(-(int)now.DayOfWeek).Date; // Sunday 00:00

        var weeklyScores = await _context.TentativasQuiz
            .Where(t => t.Completa && t.DataInicio >= startOfWeek)
            .GroupBy(t => t.UtilizadorId)
            .Select(g => new
            {
                UtilizadorId = g.Key,
                TotalScore = g.Sum(t => t.Pontuacao)
            })
            .ToListAsync();

        var users = await _context.Utilizadores
            .Select(u => new { u.Id, u.EscolaId, u.Provincia })
            .ToListAsync();

        var rankings = new List<Ranking>();
        foreach (var score in weeklyScores)
        {
            var user = users.FirstOrDefault(u => u.Id == score.UtilizadorId);
            rankings.Add(new Ranking
            {
                UtilizadorId = score.UtilizadorId,
                Pontuacao = score.TotalScore,
                Periodo = PeriodoRanking.Semanal,
                EscolaId = user?.EscolaId,
                Provincia = user?.Provincia,
                DataSnapshot = now
            });
        }

        await _context.Rankings.AddRangeAsync(rankings);
        await _context.SaveChangesAsync();
    }

    public async Task<List<Ranking>> GetRankingAsync(string tipo, PeriodoRanking periodo, int? escolaId = null, string? provincia = null)
    {
        string cacheKey = $"ranking_{tipo}_{periodo}_{escolaId}_{provincia}";

        if (_cache.TryGetValue(cacheKey, out List<Ranking>? cachedRankings))
        {
            return cachedRankings!;
        }

        var query = _context.Rankings
            .Where(r => r.Periodo == periodo);

        if (tipo == "escola" && escolaId.HasValue)
        {
            query = query.Where(r => r.EscolaId == escolaId.Value);
        }
        else if (tipo == "provincia" && !string.IsNullOrEmpty(provincia))
        {
            query = query.Where(r => r.Provincia == provincia);
        }

        var result = await query
            .OrderByDescending(r => r.Pontuacao)
            .Take(100)
            .ToListAsync();

        _cache.Set(cacheKey, result, TimeSpan.FromMinutes(5));

        return result;
    }
}
