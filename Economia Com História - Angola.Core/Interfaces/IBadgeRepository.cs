using EconomiaComHistoria.Core.Entities;

namespace EconomiaComHistoria.Core.Interfaces;

public interface IBadgeRepository : IRepository<Badge>
{
    Task<IEnumerable<Badge>> GetAllActiveAsync();
    Task<Badge?> GetByNomeAsync(string nome);
}