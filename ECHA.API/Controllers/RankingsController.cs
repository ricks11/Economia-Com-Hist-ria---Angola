using EconomiaComHistoria.Core.Enums;
using EconomiaComHistoria.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EconomiaComHistoria.API.Controllers;

[ApiController]
[Route("api/ranking")]
[Authorize]
public class RankingsController : ControllerBase
{
    private readonly IRankingService _rankingService;

    public RankingsController(IRankingService rankingService)
    {
        _rankingService = rankingService;
    }

    [HttpGet]
    public async Task<ActionResult> GetRanking(
        [FromQuery] string tipo,
        [FromQuery] PeriodoRanking periodo,
        [FromQuery] int? escolaId,
        [FromQuery] string? provincia)
    {
        var rankings = await _rankingService.GetRankingAsync(tipo, periodo, escolaId, provincia);

        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var userRank = rankings.FirstOrDefault(r => r.UtilizadorId == userId);
        int userPosition = rankings.FindIndex(r => r.UtilizadorId == userId) + 1;

        return Ok(new
        {
            Top100 = rankings,
            UserPosition = userPosition > 0 ? userPosition : 0,
            UserScore = userRank?.Pontuacao
        });
    }
}
