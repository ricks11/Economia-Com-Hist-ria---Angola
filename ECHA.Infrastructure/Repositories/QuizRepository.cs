using EconomiaComHistoria.Core.Entities;
using EconomiaComHistoria.Core.Interfaces;
using EconomiaComHistoria.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EconomiaComHistoria.Infrastructure.Repositories;

public interface IQuizRepository
{
    Task<Quiz?> GetByIdAsync(int id);
    Task<List<Quiz>> GetAvailableQuizzesAsync(string? nivel, string? tema);
    Task CreateAsync(Quiz quiz);
    Task UpdateAsync(Quiz quiz);
    Task DeleteAsync(int id);
    Task<TentativaQuiz?> GetLastAttemptAsync(int userId, int quizId);
    Task CreateAttemptAsync(TentativaQuiz tentativa);
    Task<TentativaQuiz?> GetAttemptByIdAsync(int id);
    Task AddRespostasAsync(List<RespostaPergunta> respostas);
}

public class QuizRepository : IQuizRepository
{
    private readonly AppDbContext _context;

    public QuizRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Quiz?> GetByIdAsync(int id)
    {
        return await _context.Quizzes
            .Include(q => q.Perguntas)
            .ThenInclude(p => p.Opcoes)
            .FirstOrDefaultAsync(q => q.Id == id && !q.IsDeleted);
    }

    public async Task<List<Quiz>> GetAvailableQuizzesAsync(string? nivel, string? tema)
    {
        var query = _context.Quizzes.Where(q => !q.IsDeleted);

        if (!string.IsNullOrEmpty(tema))
        {
            query = query.Where(q => q.Tema == tema);
        }

        return await query.ToListAsync();
    }

    public async Task<TentativaQuiz?> GetAttemptByIdAsync(int id)
    {
        return await _context.TentativasQuiz
            .Include(t => t.Quiz)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task AddRespostasAsync(List<RespostaPergunta> respostas)
    {
        await _context.RespostasPerguntas.AddRangeAsync(respostas);
        await _context.SaveChangesAsync();
    }

    public async Task<TentativaQuiz?> GetLastAttemptAsync(int userId, int quizId)
    {
        return await _context.TentativasQuiz
            .Where(t => t.UtilizadorId == userId && t.QuizId == quizId)
            .OrderByDescending(t => t.DataInicio)
            .FirstOrDefaultAsync();
    }

    public async Task CreateAttemptAsync(TentativaQuiz tentativa)
    {
        await _context.TentativasQuiz.AddAsync(tentativa);
        await _context.SaveChangesAsync();
    }

    public async Task CreateAsync(Quiz quiz)
    {
        await _context.Quizzes.AddAsync(quiz);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Quiz quiz)
    {
        _context.Quizzes.Update(quiz);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var quiz = await _context.Quizzes.FindAsync(id);
        if (quiz != null)
        {
            quiz.IsDeleted = true;
            await _context.SaveChangesAsync();
        }
    }
}
