using EconomiaComHistoria.Core.Entities;
using EconomiaComHistoria.Core.DTOs;
using EconomiaComHistoria.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EconomiaComHistoria.Infrastructure.Services;

public interface IPlanoEstudoService
{
    Task<PlanoEstudo> GerarPlanoAutomaticoAsync(int utilizadorId, CancellationToken ct = default);
}

public class PlanoEstudoService : IPlanoEstudoService
{
    private readonly AppDbContext _context;

    public PlanoEstudoService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PlanoEstudo> GerarPlanoAutomaticoAsync(int utilizadorId, CancellationToken ct = default)
    {
        // 1. Identificar temas com baixo desempenho (média de acerto < 60%)
        var desempenhoPorTema = await _context.RespostasPerguntas
            .Include(r => r.Pergunta)
            .ThenInclude(p => p.Quiz)
            .Where(r => r.TentativaQuiz.UtilizadorId == utilizadorId)
            .GroupBy(r => r.Pergunta.Quiz.Tema)
            .Select(g => new
            {
                Tema = g.Key,
                TaxaAcerto = (double)g.Count(r => r.IsCorrecta) / g.Count() * 100
            })
            .Where(x => x.TaxaAcerto < 60)
            .ToListAsync(ct);

        var temasCriticos = desempenhoPorTema.Select(x => x.Tema).ToList();

        // 2. Sugerir conteúdos desses temas que o usuário ainda não visualizou
        var conteudosSugeridos = await _context.Conteudos
            .Where(c => temasCriticos.Contains(c.Tema) && 
                       !_context.VisualizacoesConteudo.Any(v => v.UtilizadorId == utilizadorId && v.ConteudoId == c.Id))
            .Take(5)
            .ToListAsync(ct);

        // 3. Criar o plano de estudo
        var plano = new PlanoEstudo
        {
            UtilizadorId = utilizadorId,
            Titulo = $"Plano de Reforço - {DateTime.UtcNow:dd/MM/yyyy}",
            Descricao = "Este plano foi gerado automaticamente com base no seu desempenho em temas específicos.",
            DataInicio = DateTime.UtcNow,
            // Na estrutura atual, PlanoEstudo não tem uma lista de conteúdos sugeridos explícita,
            // mas podemos usar a Descrição ou criar uma entidade de associação.
            // Para manter simples dentro das entidades existentes, vamos listar na descrição.
        };

        if (conteudosSugeridos.Any())
        {
            plano.Descricao += "\n\nConteúdos sugeridos:";
            foreach (var c in conteudosSugeridos)
            {
                plano.Descricao += $"\n- {c.Titulo} (Tema: {c.Tema})";
            }
        }
        else
        {
            plano.Descricao += "\n\nParabéns! Você está indo bem nos temas explorados.";
        }

        _context.PlanosEstudo.Add(plano);
        await _context.SaveChangesAsync(ct);

        return plano;
    }
}
