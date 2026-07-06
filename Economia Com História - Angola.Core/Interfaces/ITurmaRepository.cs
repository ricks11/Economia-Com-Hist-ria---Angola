using EconomiaComHistoria.Core.Entities;

namespace EconomiaComHistoria.Core.Interfaces;

public interface ITurmaRepository : IRepository<Turma>
{
    Task<IEnumerable<Turma>> GetByEscolaAsync(int escolaId);
    Task<IEnumerable<Turma>> GetByProfessorAsync(int professorId);
}