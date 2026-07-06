using EconomiaComHistoria.Core.Entities;
using EconomiaComHistoria.Core.Interfaces;
using EconomiaComHistoria.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EconomiaComHistoria.Infrastructure.Repositories;

public class AuditoriaLogRepository : BaseRepository<AuditoriaLog>, IAuditoriaLogRepository
{
    public AuditoriaLogRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<AuditoriaLog>> GetByUtilizadorAsync(int utilizadorId)
        => await _dbSet
            .Where(l => l.UtilizadorId == utilizadorId)
            .OrderByDescending(l => l.Timestamp)
            .ToListAsync();

    public async Task<IEnumerable<AuditoriaLog>> GetByAcaoAsync(string acao)
        => await _dbSet
            .Where(l => l.Acao == acao)
            .OrderByDescending(l => l.Timestamp)
            .ToListAsync();

    public async Task<IEnumerable<AuditoriaLog>> GetByPeriodoAsync(DateTime inicio, DateTime fim)
        => await _dbSet
            .Where(l => l.Timestamp >= inicio && l.Timestamp <= fim)
            .OrderByDescending(l => l.Timestamp)
            .ToListAsync();
}