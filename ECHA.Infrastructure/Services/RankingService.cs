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

    public async Task GerarSnapshotSemanalAsync(CancellationToken ct = default)
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
            .ToListAsync(ct);

        var users = await _context.Utilizadores
            .Select(u => new { u.Id, u.EscolaId, u.Provincia, u.Municipio })
            .ToListAsync(ct);

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
        await _context.Rankings.AddAsync(ranking, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<List<EntradaRanking>> GetRankingAsync(
    TipoRanking tipo,
    PeriodoRanking periodo,
    int? escolaId = null,
    string? provincia = null,
    string? municipio = null,
    CancellationToken ct = default)
    {
        string cacheKey = $"ranking_{tipo}_{periodo}_{escolaId}_{provincia}_{municipio}";

        if (_cache.TryGetValue(cacheKey, out List<EntradaRanking>? cachedRankings))
        {
            return cachedRankings!;
        }

        // 1. Obter o ID do ranking mais recente (apenas o ID, sem carregar entradas)
        var latestRankingId = await _context.Rankings
            .Where(r => r.Periodo == periodo)
            .OrderByDescending(r => r.DataCalculo)
            .Select(r => r.Id)
            .FirstOrDefaultAsync(ct);

        if (latestRankingId == 0)
        {
            return new List<EntradaRanking>();
        }

        // 2. Construir a consulta sobre EntradaRanking (DIRETAMENTE sobre o DbSet)
        var query = _context.EntradasRanking
            .Include(e => e.Utilizador)
            .Include(e => e.Escola)
            .Where(e => e.RankingId == latestRankingId);

        // 3. Aplicar filtros (IQueryable nativo, sem materialização)
        if (tipo == TipoRanking.Escola && escolaId.HasValue)
        {
            query = query.Where(e => e.EscolaId == escolaId.Value);
        }
        else if (tipo == TipoRanking.Provincia && !string.IsNullOrEmpty(provincia))
        {
            query = query.Where(e => e.Utilizador != null && e.Utilizador.Provincia == provincia);
        }
        else if (tipo == TipoRanking.Municipio && !string.IsNullOrEmpty(municipio))
        {
            query = query.Where(e => e.Utilizador != null && e.Utilizador.Municipio == municipio);
        }

        // 4. Executar a consulta assíncrona (agora é uma IQueryable do EF Core)
        var result = await query
            .OrderBy(e => e.Posicao)
            .Take(100)
            .ToListAsync(ct);

        // 5. Guardar em cache
        _cache.Set(cacheKey, result, TimeSpan.FromMinutes(5));

        return result;
    }
}