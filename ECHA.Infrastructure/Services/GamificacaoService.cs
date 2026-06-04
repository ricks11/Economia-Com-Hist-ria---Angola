using EconomiaComHistoria.Core.Entities;
using EconomiaComHistoria.Core.Enums;
using EconomiaComHistoria.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EconomiaComHistoria.Infrastructure.Services;

public interface IGamificacaoService
{
    Task ProcessarEventoAsync(int utilizadorId, TipoEventoGamificacao tipo, string descricao, int pontos, CancellationToken ct = default);
    Task<List<Badge>> GetBadgesDisponiveisAsync(CancellationToken ct = default);
    Task<object> GetMetricasEngajamentoAsync(CancellationToken ct = default);
}

public class GamificacaoService : IGamificacaoService
{
    private readonly AppDbContext _context;

    public GamificacaoService(AppDbContext context)
    {
        _context = context;
    }

    public async Task ProcessarEventoAsync(int utilizadorId, TipoEventoGamificacao tipo, string descricao, int pontos, CancellationToken ct = default)
    {
        var user = await _context.Utilizadores.FindAsync(new object[] { utilizadorId }, ct);
        if (user == null) return;

        // Registrar evento
        var evento = new EventoGamificacao
        {
            UtilizadorId = utilizadorId,
            Tipo = tipo,
            Descricao = descricao,
            PontosGanhos = pontos,
            DataEvento = DateTime.UtcNow
        };
        _context.EventosGamificacao.Add(evento);

        // Atualizar pontos do usuário
        user.PontosTotais += pontos;

        // Verificar novos badges
        await VerificarBadgesAsync(user, ct);

        await _context.SaveChangesAsync(ct);
    }

    private async Task VerificarBadgesAsync(Utilizador user, CancellationToken ct)
    {
        var badgesDisponiveis = await _context.Badges
            .Where(b => !user.BadgesConquistados.Any(bc => bc.BadgeId == b.Id))
            .ToListAsync(ct);

        foreach (var badge in badgesDisponiveis)
        {
            bool conquistou = badge.Criterio switch
            {
                CriterioBadge.PontosAtingidos => user.PontosTotais >= badge.ValorCriterio,
                CriterioBadge.QuizzesCompletados => await _context.TentativasQuiz.CountAsync(t => t.UtilizadorId == user.Id && t.Completa, ct) >= badge.ValorCriterio,
                CriterioBadge.StreakAtingido => user.StreakAtual >= badge.ValorCriterio,
                CriterioBadge.ConteudosExplorados => await _context.VisualizacoesConteudo.CountAsync(v => v.UtilizadorId == user.Id, ct) >= badge.ValorCriterio,
                _ => false
            };

            if (conquistou)
            {
                _context.BadgesConquistados.Add(new BadgeConquistado
                {
                    UtilizadorId = user.Id,
                    BadgeId = badge.Id,
                    DataConquista = DateTime.UtcNow
                });
            }
        }
    }

    public async Task<List<Badge>> GetBadgesDisponiveisAsync(CancellationToken ct = default)
    {
        return await _context.Badges.ToListAsync(ct);
    }

    public async Task<object> GetMetricasEngajamentoAsync(CancellationToken ct = default)
    {
        var totalUsers = await _context.Utilizadores.CountAsync(ct);
        if (totalUsers == 0) return new { };

        var badges = await _context.Badges
            .Select(b => new
            {
                b.Nome,
                Percentagem = (double)b.BadgesConquistados.Count / totalUsers * 100
            })
            .ToListAsync(ct);

        var mediaStreak = await _context.Utilizadores.AverageAsync(u => (double)u.StreakAtual, ct);

        return new
        {
            BadgesPorUtilizador = badges,
            MediaStreak = mediaStreak
        };
    }
}
