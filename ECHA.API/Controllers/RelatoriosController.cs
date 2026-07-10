using EconomiaComHistoria.Core.DTOs;
using EconomiaComHistoria.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ECHA.API.Controllers;

[ApiController]
[Route("api/relatorios")]
[Authorize(Roles = "Admin,Editor")]
public class RelatoriosController : ControllerBase
{
    private readonly IRelatorioService _relatorioService;

    public RelatoriosController(IRelatorioService relatorioService)
    {
        _relatorioService = relatorioService;
    }

    [HttpPost("gerar")]
    public async Task<ActionResult<RelatorioStatusDto>> GerarRelatorio([FromBody] SolicitarRelatorioDto request, CancellationToken ct)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out var userId)) return Unauthorized();

        var status = await _relatorioService.SolicitarRelatorioAsync(userId, request, ct);
        return Ok(status);
    }

    [HttpGet("{id}/status")]
    public async Task<ActionResult<RelatorioStatusDto>> GetStatus(int id, CancellationToken ct)
    {
        var status = await _relatorioService.GetStatusAsync(id, ct);
        if (status == null) return NotFound();
        return Ok(status);
    }

    [HttpGet("{id}/download")]
    public async Task<IActionResult> Download(int id, CancellationToken ct)
    {
        // TODO: Implement actual download logic
        return Ok(new { message = "Download starting..." });
    }
}
