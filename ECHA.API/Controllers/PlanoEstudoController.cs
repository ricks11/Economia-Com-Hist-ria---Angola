using EconomiaComHistoria.Core.Entities;
using EconomiaComHistoria.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ECHA.API.Controllers;

[ApiController]
[Route("api/plano-estudo")]
[Authorize]
public class PlanoEstudoController : ControllerBase
{
    private readonly IPlanoEstudoService _planoEstudoService;

    public PlanoEstudoController(IPlanoEstudoService planoEstudoService)
    {
        _planoEstudoService = planoEstudoService;
    }

    [HttpPost("gerar")]
    public async Task<ActionResult<PlanoEstudo>> GerarPlano(CancellationToken ct)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out var userId)) return Unauthorized();

        var plano = await _planoEstudoService.GerarPlanoAutomaticoAsync(userId, ct);
        return Ok(plano);
    }
}
