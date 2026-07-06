using EconomiaComHistoria.Core.Entities;
using EconomiaComHistoria.Core.Interfaces;
using EconomiaComHistoria.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EconomiaComHistoria.Infrastructure.Repositories;

public class PlanoEstudoRepository : BaseRepository<PlanoEstudo>, IPlanoEstudoRepository
{
    public PlanoEstudoRepository(AppDbContext context) : base(context) { }

    public async Task<PlanoEstudo?> GetAtivoByUtilizadorAsync(int utilizadorId)
        => await _dbSet.Where(p => p.UtilizadorId == utilizadorId)
            .OrderByDescending(p => p.DataInicio)
            .FirstOrDefaultAsync();
}
