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
        var startOfWeek = now.AddDays(-(int)now.DayOfWeek).Date;

        var weeklyScores = await _context.TentativasQuiz
            .Where(t => t.Completada && t.DataHora >= startOfWeek)
            .GroupBy(t => t.UtilizadorId)
            .Select(g => new
            {
                UtilizadorId = g.Key,
                TotalScore = g.Sum(t => t.Pontuacao),
                QuizzesCompletados = g.Count()
            })
            .ToListAsync();

        var users = await _context.Utilizadores
            .Select(u => new { u.Id, u.EscolaId, u.Provincia })
            .ToListAsync();

        var ranking = new Ranking
        {
            Tipo = TipoRanking.Nacional,
            Periodo = PeriodoRanking.Semanal,
            DataCalculo = now
        };

        var entradas = new List<EntradaRanking>();
        int posicao = 1;
        foreach (var score in weeklyScores.OrderByDescending(s => s.TotalScore))
        {
            var user = users.FirstOrDefault(u => u.Id == score.UtilizadorId);
            entradas.Add(new EntradaRanking
            {
                Posicao = posicao++,
                Pontos = score.TotalScore,
                QuizzesCompletados = score.QuizzesCompletados,
                UtilizadorId = score.UtilizadorId,
                EscolaId = user?.EscolaId,
                Ranking = ranking
            });
        }

        ranking.Entradas = entradas;
        await _context.Rankings.AddAsync(ranking);
        await _context.SaveChangesAsync();
    }

    public async Task<List<EntradaRanking>> GetRankingAsync(TipoRanking tipo, PeriodoRanking periodo, int? escolaId = null, string? provincia = null)
    {
        string cacheKey = $"ranking_{tipo}_{periodo}_{escolaId}_{provincia}";

        if (_cache.TryGetValue(cacheKey, out List<EntradaRanking>? cachedRankings))
        {
            return cachedRankings!;
        }

        var rankingQuery = _context.Rankings
            .Include(r => r.Entradas)
            .ThenInclude(e => e.Utilizador)
            .Where(r => r.Periodo == periodo);

        // Find the most recent ranking for this period
        var latestRanking = await rankingQuery
            .OrderByDescending(r => r.DataCalculo)
            .FirstOrDefaultAsync();

        if (latestRanking == null)
        {
            return new List<EntradaRanking>();
        }

        IQueryable<EntradaRanking> query = latestRanking.Entradas.AsQueryable();

        if (tipo == TipoRanking.Escola && escolaId.HasValue)
        {
            query = query.Where(e => e.EscolaId == escolaId.Value);
        }
        else if (tipo == TipoRanking.Provincia && !string.IsNullOrEmpty(provincia))
        {
            query = query.Where(e => e.Utilizador != null && e.Utilizador.Provincia == provincia);
        }

        var result = await query
            .OrderBy(e => e.Posicao)
            .Take(100)
            .ToListAsync();

        _cache.Set(cacheKey, result, TimeSpan.FromMinutes(5));

        return result;
    }
}