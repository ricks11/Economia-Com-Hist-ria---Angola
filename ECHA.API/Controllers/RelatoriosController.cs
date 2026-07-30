using EconomiaComHistoria.Core.DTOs;
using EconomiaComHistoria.Core.Enums;
using EconomiaComHistoria.Core.Interfaces;
using EconomiaComHistoria.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ECHA.API.Controllers;

[ApiController]
[Route("api/relatorios")]
[Authorize(Roles = "Admin,Editor,SuperAdmin")]
public class RelatoriosController : ControllerBase
{
    private readonly IRelatorioService _relatorioService;
    private readonly IAuditoriaService _auditoriaService;

    public RelatoriosController(IRelatorioService relatorioService, IAuditoriaService auditoriaService)
    {
        _relatorioService = relatorioService;
        _auditoriaService = auditoriaService;
    }

    private bool TryGetUserId(out int userId)
    {
        var value = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                 ?? User.FindFirst("sub")?.Value;
        return int.TryParse(value, out userId);
    }

    [HttpGet]
    public async Task<ActionResult<List<RelatorioListaDto>>> ListarRelatorios(
        [FromQuery] int? escolaId,
        CancellationToken ct)
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized();

        var lista = await _relatorioService.ListarRelatoriosAsync(userId, escolaId, ct);
        return Ok(lista);
    }

    [HttpPost("gerar")]
    public async Task<ActionResult<RelatorioStatusDto>> GerarRelatorio(
        [FromBody] SolicitarRelatorioDto request,
        CancellationToken ct)
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized();

        if (string.IsNullOrWhiteSpace(request.Titulo))
            return BadRequest(new { message = "Título é obrigatório." });

        // Validar que a data de fim não é anterior à data de início
        if (request.Fim.HasValue && request.Inicio.HasValue && request.Fim.Value < request.Inicio.Value)
            return BadRequest(new { message = "A data de fim não pode ser anterior à data de início." });

        // Validar que a data de fim não seja anterior à data de início
        if (request.Fim.HasValue && request.Inicio.HasValue && request.Fim.Value < request.Inicio.Value)
            return BadRequest(new { message = "A data de fim não pode ser anterior à data de início." });

        var status = await _relatorioService.SolicitarRelatorioAsync(userId, request, ct);
        await _auditoriaService.RegistarAsync(
            userId,
            "SolicitarRelatorio",
            "Relatorio",
            status.Id,
            null,
            $"Título: {request.Titulo}, Tipo: {request.Tipo}",
            HttpContext
        );
        return CreatedAtAction(nameof(GetStatus), new { id = status.Id }, status);
    }

    [HttpGet("{id}/status")]
    public async Task<ActionResult<RelatorioStatusDto>> GetStatus(int id, CancellationToken ct)
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized();

        var status = await _relatorioService.GetStatusAsync(id, ct);
        if (status == null)
            return NotFound();

        return Ok(status);
    }

    [HttpGet("{id}/download")]
    public async Task<IActionResult> Download(int id, CancellationToken ct)
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized();

        var status = await _relatorioService.GetStatusAsync(id, ct);
        if (status == null || status.Estado != EstadoRelatorio.Concluido)
            return NotFound(new { message = "Relatório não encontrado ou não está pronto para download." });

        var dados = await _relatorioService.DownloadRelatorioAsync(id, ct);
        if (dados == null)
            return NotFound(new { message = "Ficheiro não encontrado." });

        // Sempre retornar como CSV agora que geramos apenas CSV
        var nomeFicheiro = $"relatorio_{id}.csv";

        await _auditoriaService.RegistarAsync(
            userId,
            "DescarregarRelatorio",
            "Relatorio",
            id,
            null,
            null,
            HttpContext
        );

        return File(dados, "text/csv", nomeFicheiro);
    }
}