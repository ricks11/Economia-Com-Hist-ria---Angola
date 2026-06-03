using EconomiaComHistoria.Core.Entities;
using EconomiaComHistoria.Core.Enums;
using EconomiaComHistoria.Core.Interfaces;
using EconomiaComHistoria.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace EconomiaComHistoria.Infrastructure.Repositories;



public class QuizRepository : IQuizRepository
{
    private readonly AppDbContext _context;

    public QuizRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Quiz?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Quizzes
            .Include(q => q.Perguntas)
                .ThenInclude(p => p.Opcoes)
            .FirstOrDefaultAsync(q => q.Id == id && q.Ativo, cancellationToken);
    }

    public async Task<List<Quiz>> GetAvailableQuizzesAsync(NivelDificuldade? nivel, string? tema, CancellationToken cancellationToken = default)
    {
        var query = _context.Quizzes.Where(q => q.Ativo);

        if (nivel.HasValue)
        {
            query = query.Where(q => q.Nivel == nivel.Value);
        }

        if (!string.IsNullOrEmpty(tema))
        {
            query = query.Where(q => q.Tema == tema);
        }

        return await query.ToListAsync();
    }

    public async Task<TentativaQuiz?> GetAttemptByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.TentativasQuiz
            .Include(t => t.Quiz)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async Task AddRespostasAsync(List<RespostaPergunta> respostas, CancellationToken cancellationToken = default)
    {
        await _context.RespostasPerguntas.AddRangeAsync(respostas);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<TentativaQuiz?> GetLastAttemptAsync(int userId, int quizId, CancellationToken cancellationToken = default)
    {
        return await _context.TentativasQuiz
            .Where(t => t.UtilizadorId == userId && t.QuizId == quizId)
            .OrderByDescending(t => t.DataHora)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task CreateAttemptAsync(TentativaQuiz tentativa, CancellationToken cancellationToken = default)
    {
        await _context.TentativasQuiz.AddAsync(tentativa);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task CreateAsync(Quiz quiz, CancellationToken cancellationToken = default)
    {
        await _context.Quizzes.AddAsync(quiz);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Quiz quiz, CancellationToken cancellationToken = default)
    {
        _context.Quizzes.Update(quiz);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var quiz = await _context.Quizzes.FindAsync(id);
        if (quiz != null)
        {
            quiz.Ativo = false;
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}