using EconomiaComHistoria.Core.Entities;
using EconomiaComHistoria.Core.Interfaces;
using EconomiaComHistoria.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EconomiaComHistoria.Infrastructure.Repositories;

public class EscolaRepository : BaseRepository<Escola>, IEscolaRepository
{
    public EscolaRepository(AppDbContext context) : base(context) { }

    public async Task<Escola?> GetByCodigoConviteAsync(string codigo)
        => await _dbSet.FirstOrDefaultAsync(e => e.CodigoConvite == codigo);

    public async Task<IEnumerable<Escola>> GetByProvinciaAsync(string provincia)
        => await _dbSet.Where(e => e.Provincia == provincia)
            .OrderBy(e => e.Nome)
            .ToListAsync();
}
