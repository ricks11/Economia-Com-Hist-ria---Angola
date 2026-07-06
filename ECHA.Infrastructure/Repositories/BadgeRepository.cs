using EconomiaComHistoria.Core.Entities;
using EconomiaComHistoria.Core.Interfaces;
using EconomiaComHistoria.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EconomiaComHistoria.Infrastructure.Repositories;

public class BadgeRepository : BaseRepository<Badge>, IBadgeRepository
{
    public BadgeRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<Badge>> GetAllActiveAsync()
        => await _dbSet.Where(b => b.Ativo).ToListAsync();

    public async Task<Badge?> GetByNomeAsync(string nome)
        => await _dbSet.FirstOrDefaultAsync(b => b.Nome == nome);
}
