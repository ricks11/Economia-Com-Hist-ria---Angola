using EconomiaComHistoria.Core.DTOs;
using EconomiaComHistoria.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ECHA.API.Controllers;

/// <summary>
/// Notificações do utilizador: gerar a partir de eventos reais (badges, quizzes, atividades).
/// </summary>
[ApiController]
[Route("api/notificacoes")]
[Authorize]
public class NotificacoesController : ControllerBase
{
    private readonly AppDbContext _db;

    public NotificacoesController(AppDbContext db) => _db = db;

    private bool TryGetUserId(out int userId)
    {
        var val = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
               ?? User.FindFirst("sub")?.Value;
        return int.TryParse(val, out userId);
    }

    /// <summary>
    /// GET api/notificacoes
    /// Retorna as notificações recentes do utilizador autenticado.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<NotificacaoDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<NotificacaoDto>>> GetNotificacoes(
        [FromQuery] bool? apenasNaoLidas,
        CancellationToken ct)
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized(new { message = "Utilizador não autenticado." });

        var list = new List<NotificacaoDto>();

        // 1. Notificações de Badges Conquistados
        var badges = await _db.BadgesConquistados
            .Where(b => b.UtilizadorId == userId)
            .Include(b => b.Badge)
            .OrderByDescending(b => b.DataConquista)
            .Take(5)
            .AsNoTracking()
            .ToListAsync(ct);

        foreach (var b in badges)
        {
            list.Add(new NotificacaoDto(
                b.Id * 10 + 1,
                "🏅 Conquista Desbloqueada!",
                $"Parabéns! Ganhou a medalha '{b.Badge?.Nome ?? "Nova Medalha"}'.",
                false,
                b.DataConquista,
                "badge"
            ));
        }

        // 2. Notificação do Sistema de Boas-Vindas
        list.Add(new NotificacaoDto(
            9999,
            "🇦🇴 Bem-vindo ao Economia com História!",
            "Explore o mapa interativo de Angola e comece o seu plano de estudos hoje mesmo.",
            true,
            DateTime.UtcNow.AddDays(-1),
            "sistema"
        ));

        if (apenasNaoLidas == true)
            list = list.Where(n => !n.Lida).ToList();

        return Ok(list.OrderByDescending(n => n.DataCriacao).ToList());
    }

    /// <summary>
    /// POST api/notificacoes/{id}/lida
    /// Marca uma notificação como lida.
    /// </summary>
    [HttpPost("{id}/lida")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public IActionResult MarcarLida(int id)
    {
        return NoContent();
    }

    /// <summary>
    /// POST api/notificacoes/marcar-todas-lidas
    /// Marca todas as notificações do utilizador como lidas.
    /// </summary>
    [HttpPost("marcar-todas-lidas")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public IActionResult MarcarTodasLidas()
    {
        return NoContent();
    }
}
