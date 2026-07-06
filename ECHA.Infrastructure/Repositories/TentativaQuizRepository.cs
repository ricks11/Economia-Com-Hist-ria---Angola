using EconomiaComHistoria.Core.Entities;
using EconomiaComHistoria.Core.Interfaces;
using EconomiaComHistoria.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EconomiaComHistoria.Infrastructure.Repositories;

public class TentativaQuizRepository : BaseRepository<TentativaQuiz>, ITentativaQuizRepository
{
    public TentativaQuizRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<TentativaQuiz>> GetByUtilizadorAsync(int utilizadorId)
        => await _dbSet.Where(t => t.UtilizadorId == utilizadorId)
            .OrderByDescending(t => t.DataHora)
            .ToListAsync();

    public async Task<IEnumerable<TentativaQuiz>> GetByQuizAsync(int quizId)
        => await _dbSet.Where(t => t.QuizId == quizId)
            .OrderByDescending(t => t.DataHora)
            .ToListAsync();

    public async Task<TentativaQuiz?> GetUltimaTentativaElegivelAsync(int utilizadorId, int quizId)
        => await _dbSet
            .Where(t => t.UtilizadorId == utilizadorId && t.QuizId == quizId && t.ElegivelRanking)
            .OrderByDescending(t => t.DataHora)
            .FirstOrDefaultAsync();

    public async Task<int> CountCompletasByUserAsync(int utilizadorId)
        => await _dbSet.CountAsync(t => t.UtilizadorId == utilizadorId && t.Completada);
}
