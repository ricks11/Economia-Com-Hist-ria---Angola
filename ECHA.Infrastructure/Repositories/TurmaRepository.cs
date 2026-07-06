using EconomiaComHistoria.Core.Entities;
using EconomiaComHistoria.Core.Interfaces;
using EconomiaComHistoria.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EconomiaComHistoria.Infrastructure.Repositories;

public class TurmaRepository : BaseRepository<Turma>, ITurmaRepository
{
    public TurmaRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<Turma>> GetByEscolaAsync(int escolaId)
        => await _dbSet.Where(t => t.EscolaId == escolaId)
            .Include(t => t.Professor)
            .OrderBy(t => t.Ano)
            .ToListAsync();

    public async Task<IEnumerable<Turma>> GetByProfessorAsync(int professorId)
        => await _dbSet.Where(t => t.ProfessorId == professorId)
            .Include(t => t.Escola)
            .OrderBy(t => t.Ano)
            .ToListAsync();
}
