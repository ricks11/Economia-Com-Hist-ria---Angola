using EconomiaComHistoria.Core.Entities;

namespace EconomiaComHistoria.Core.Interfaces;

public interface IRelatorioRepository : IRepository<RelatorioProgresso>
{
    Task<IEnumerable<RelatorioProgresso>> GetByEscolaAsync(int escolaId);
    Task<IEnumerable<RelatorioProgresso>> GetByTurmaAsync(int turmaId);
    Task<IEnumerable<RelatorioProgresso>> GetPendentesAsync();
}