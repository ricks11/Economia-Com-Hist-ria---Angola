using EconomiaComHistoria.Core.DTOs;
using EconomiaComHistoria.Core.Entities;
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
    [ResponseCache(Duration = 300, VaryByQueryKeys = new[] { "tipo", "periodo", "escolaId", "provincia" })]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> GetRanking(
        [FromQuery] TipoRanking tipo,
        [FromQuery] PeriodoRanking periodo,
        [FromQuery] int? escolaId,
        [FromQuery] string? provincia,
        CancellationToken cancellationToken)
    {
        // Valida combinação tipo + filtro
        if (tipo == TipoRanking.Escola && !escolaId.HasValue)
            return BadRequest(new { message = "escolaId é obrigatório para ranking por escola" });

        if (tipo == TipoRanking.Provincia && string.IsNullOrWhiteSpace(provincia))
            return BadRequest(new { message = "provincia é obrigatória para ranking por província" });

        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier)
                     ?? User.FindFirstValue("sub");
        if (!int.TryParse(userIdStr, out var userId))
            return Unauthorized(new { message = "Utilizador não autenticado" });

        var entradas = await _rankingService
            .GetRankingAsync(tipo, periodo, escolaId, provincia);

        var dtos = entradas.Select(e => new RankingEntradaDto(
            e.Posicao,
            e.UtilizadorId,
            e.Utilizador?.Nome ?? string.Empty,
            e.Pontos,
            e.QuizzesCompletados,
            e.Escola?.Nome)).ToList();

        // Posição do utilizador autenticado
        var userIndex = dtos.FindIndex(r => r.UtilizadorId == userId);
        var userPosition = userIndex >= 0 ? userIndex + 1 : 0;
        var userEntry = userIndex >= 0 ? dtos[userIndex] : null;

        var response = new RankingResponseDto
        {
            Top100 = dtos,
            PosicaoUtilizador = userPosition,
            PontosUtilizador = userEntry?.Pontos,
            Tipo = tipo.ToString(),
            Periodo = periodo.ToString()
        };

        return Ok(response);
    }

    [HttpGet("semanal/gerar")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GerarSnapshotManual(CancellationToken cancellationToken)
    {
        await _rankingService.GerarSnapshotSemanalAsync();
        return Ok(new { message = "Snapshot semanal gerado com sucesso" });
    }
}