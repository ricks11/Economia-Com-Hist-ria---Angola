using EconomiaComHistoria.Core.Entities;

namespace EconomiaComHistoria.Core.Interfaces;

public interface IPlanoEstudoRepository : IRepository<PlanoEstudo>
{
    Task<PlanoEstudo?> GetAtivoByUtilizadorAsync(int utilizadorId);
}