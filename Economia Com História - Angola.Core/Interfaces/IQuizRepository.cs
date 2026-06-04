using EconomiaComHistoria.Core.Entities;
using EconomiaComHistoria.Core.Enums;

namespace EconomiaComHistoria.Core.Interfaces;

public interface IQuizRepository
{
    Task<Quiz?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<List<Quiz>> GetAvailableQuizzesAsync(NivelDificuldade? nivel, string? tema, CancellationToken cancellationToken = default);
    Task CreateAsync(Quiz quiz, CancellationToken cancellationToken = default);
    Task UpdateAsync(Quiz quiz, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<TentativaQuiz?> GetLastAttemptAsync(int userId, int quizId, CancellationToken cancellationToken = default);
    Task CreateAttemptAsync(TentativaQuiz tentativa, CancellationToken cancellationToken = default);
    Task<TentativaQuiz?> GetAttemptByIdAsync(int id, CancellationToken cancellationToken = default);
    Task AddRespostasAsync(List<RespostaPergunta> respostas, CancellationToken cancellationToken = default);
}
