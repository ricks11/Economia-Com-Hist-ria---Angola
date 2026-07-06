using EconomiaComHistoria.Core.Entities;
using EconomiaComHistoria.Core.Interfaces;
using EconomiaComHistoria.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EconomiaComHistoria.Infrastructure.Repositories;

public class RelatorioRepository : BaseRepository<RelatorioProgresso>, IRelatorioRepository
{
    public RelatorioRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<RelatorioProgresso>> GetByEscolaAsync(int escolaId)
        => await _dbSet.Where(r => r.EscolaId == escolaId)
            .OrderByDescending(r => r.DataSolicitacao)
            .ToListAsync();

    public async Task<IEnumerable<RelatorioProgresso>> GetByTurmaAsync(int turmaId)
        => await _dbSet.Where(r => r.TurmaId == turmaId)
            .OrderByDescending(r => r.DataSolicitacao)
            .ToListAsync();

    public async Task<IEnumerable<RelatorioProgresso>> GetPendentesAsync()
        => await _dbSet.Where(r => r.Estado == Core.Enums.EstadoRelatorio.Pendente)
            .OrderBy(r => r.DataSolicitacao)
            .ToListAsync();
}
