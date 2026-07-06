using EconomiaComHistoria.Core.Enums;
using EconomiaComHistoria.Core.Interfaces;
using EconomiaComHistoria.Core.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECHA.API.Controllers;

[Authorize(Roles = "SuperAdmin")]
[ApiController]
[Route("api/admin")]
public class AdminController : ControllerBase
{
    private readonly IAuditoriaService _auditoriaService;
    private readonly IUtilizadorRepository _userRepo;

    public AdminController(IAuditoriaService auditoriaService, IUtilizadorRepository userRepo)
    {
        _auditoriaService = auditoriaService;
        _userRepo = userRepo;
    }

    [HttpGet("auditoria")]
    public async Task<IActionResult> GetAuditoria([FromQuery] int? utilizadorId, [FromQuery] string? acao, [FromQuery] DateTime? inicio, [FromQuery] DateTime? fim)
    {
        var logs = await _auditoriaService.ObterLogs(utilizadorId, acao, inicio, fim);
        return Ok(logs);
    }

    [HttpPut("utilizadores/{id}/role")]
    public async Task<IActionResult> AlterarRole(int id, [FromBody] RoleChangeDto dto)
    {
        if (dto == null)
            return BadRequest("Dados da role não fornecidos.");

        var novaRole = dto.NovaRole;
        if (!Enum.TryParse(typeof(EconomiaComHistoria.Core.Enums.TipoUtilizador), novaRole, true, out var roleObj))
            return BadRequest("Role inválida.");
        var role = (EconomiaComHistoria.Core.Enums.TipoUtilizador)roleObj;

        var user = await _userRepo.GetByIdAsync(id);
        if (user == null) return NotFound();

        var antes = user.Tipo.ToString();
        user.Tipo = role;
        await _userRepo.UpdateAsync(user);

        // Regista auditoria
        await _auditoriaService.RegistarAsync(user.Id, "AlterarRole", "Utilizador", id, antes, role.ToString(), HttpContext);

        return Ok();
    }
}
