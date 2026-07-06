using EconomiaComHistoria.Core.Entities;
using EconomiaComHistoria.Core.Interfaces;
using EconomiaComHistoria.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EconomiaComHistoria.Infrastructure.Repositories;

public class PropostaQuizRepository : BaseRepository<PropostaQuiz>, IPropostaQuizRepository
{
    public PropostaQuizRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<PropostaQuiz>> GetPendentesAsync()
        => await _dbSet
            .Where(p => p.Status == "Pendente")
            .Include(p => p.Utilizador)
            .Include(p => p.TopicoForum)
            .Include(p => p.Perguntas)
            .OrderBy(p => p.DataProposta)
            .ToListAsync();

    public async Task<IEnumerable<PropostaQuiz>> GetByUtilizadorAsync(int utilizadorId)
        => await _dbSet
            .Where(p => p.UtilizadorId == utilizadorId)
            .Include(p => p.Perguntas)
            .ToListAsync();

    public async Task<IEnumerable<PropostaQuiz>> GetByTopicoForumAsync(int topicoId)
        => await _dbSet
            .Where(p => p.TopicoForumId == topicoId)
            .Include(p => p.Utilizador)
            .Include(p => p.Perguntas)
            .ToListAsync();
}