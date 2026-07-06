using EconomiaComHistoria.Core.Entities;
using EconomiaComHistoria.Core.Interfaces;
using EconomiaComHistoria.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EconomiaComHistoria.Infrastructure.Repositories;

public class UtilizadorRepository : BaseRepository<Utilizador>, IUtilizadorRepository
{
    public UtilizadorRepository(AppDbContext context) : base(context) { }

    public async Task<Utilizador?> GetByEmailAsync(string email)
        => await _dbSet.FirstOrDefaultAsync(u => u.Email == email);

    public async Task<IEnumerable<Utilizador>> GetByRoleAsync(string role)
        => await _dbSet.Where(u => u.Tipo.ToString() == role)
            .OrderBy(u => u.Nome)
            .ToListAsync();

    public async Task<IEnumerable<Utilizador>> GetByEscolaAsync(int escolaId)
        => await _dbSet.Where(u => u.EscolaId == escolaId)
            .OrderBy(u => u.Nome)
            .ToListAsync();
}
