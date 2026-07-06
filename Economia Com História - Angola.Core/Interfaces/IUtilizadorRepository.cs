using EconomiaComHistoria.Core.Entities;

namespace EconomiaComHistoria.Core.Interfaces;

public interface IUtilizadorRepository : IRepository<Utilizador>
{
    Task<Utilizador?> GetByEmailAsync(string email);
    Task<IEnumerable<Utilizador>> GetByRoleAsync(string role);
    Task<IEnumerable<Utilizador>> GetByEscolaAsync(int escolaId);
}
