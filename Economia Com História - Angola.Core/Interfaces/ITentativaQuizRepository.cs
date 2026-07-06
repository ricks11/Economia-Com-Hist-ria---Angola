using EconomiaComHistoria.Core.Entities;

namespace EconomiaComHistoria.Core.Interfaces;

public interface ITentativaQuizRepository : IRepository<TentativaQuiz>
{
    Task<IEnumerable<TentativaQuiz>> GetByUtilizadorAsync(int utilizadorId);
    Task<IEnumerable<TentativaQuiz>> GetByQuizAsync(int quizId);
    Task<TentativaQuiz?> GetUltimaTentativaElegivelAsync(int utilizadorId, int quizId);
    Task<int> CountCompletasByUserAsync(int utilizadorId);
}