using EconomiaComHistoria.Core.Entities;

namespace EconomiaComHistoria.Core.Interfaces;

public interface IEscolaRepository : IRepository<Escola>
{
    Task<Escola?> GetByCodigoConviteAsync(string codigo);
    Task<IEnumerable<Escola>> GetByProvinciaAsync(string provincia);
}