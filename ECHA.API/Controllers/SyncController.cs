using EconomiaComHistoria.Core.DTOs.Sync;
using EconomiaComHistoria.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ECHA.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]

public class SyncController : ControllerBase
{
    private readonly ISincronizacaoService _syncService;

    public SyncController(ISincronizacaoService syncService)
    {
        _syncService = syncService;
    }

    [HttpPost("tentativas")]
    public async Task<ActionResult<LoteSincronizacaoResponse>> SyncTentativas([FromBody] LoteSincronizacaoRequest request)
    {
        var utilizadorId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var response = await _syncService.ProcessarLoteAsync(utilizadorId, request);
        return Ok(response);
    }
}
