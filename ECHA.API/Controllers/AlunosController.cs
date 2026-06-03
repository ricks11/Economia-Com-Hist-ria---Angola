using EconomiaComHistoria.Core.DTOs;
using EconomiaComHistoria.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ECHA.API.Controllers;

[ApiController]
[Route("api/alunos")]
[Authorize]
public class AlunosController : ControllerBase
{
    private readonly IEscolaService _escolaService;

    public AlunosController(IEscolaService escolaService)
    {
        _escolaService = escolaService;
    }

    [HttpPost("associar")]
    public async Task<IActionResult> AssociarEscola([FromQuery] string codigo, CancellationToken ct)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out var userId)) return Unauthorized();

        var success = await _escolaService.AssociarAlunoAsync(userId, codigo, ct);
        if (success) return Ok(new { message = "Associado com sucesso" });
        
        return BadRequest(new { message = "Código inválido ou expirado" });
    }
}
