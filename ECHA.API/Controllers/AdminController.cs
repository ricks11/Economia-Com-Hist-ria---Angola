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
    private readonly ISeedService _seedService;

    public AdminController(IAuditoriaService auditoriaService, IUtilizadorRepository userRepo, ISeedService seedService)
    {
        _auditoriaService = auditoriaService;
        _userRepo = userRepo;
        _seedService = seedService;
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

    [HttpPost("seed")]
    [AllowAnonymous]
    public async Task<IActionResult> Seed()
    {
        var result = await _seedService.SeedDataAsync();
        return Ok(new { message = result });
    }

    [HttpPut("utilizadores/email/{email}/role/{role}")]
    [AllowAnonymous]
    public async Task<IActionResult> AlterarRoleByEmail(string email, string role)
    {
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(role))
            return BadRequest("Email e role são obrigatórios.");

        if (!Enum.TryParse(typeof(EconomiaComHistoria.Core.Enums.TipoUtilizador), role, true, out var roleObj))
            return BadRequest("Role inválida.");

        var user = await _userRepo.GetByEmailAsync(email);
        if (user == null) return NotFound("Utilizador não encontrado.");

        var antes = user.Tipo.ToString();
        user.Tipo = (EconomiaComHistoria.Core.Enums.TipoUtilizador)roleObj;
        await _userRepo.UpdateAsync(user);
        await _auditoriaService.RegistarAsync(
            user.Id,
            "AlterarRole",
            "Utilizador",
            user.Id,
            antes,
            role,
            HttpContext
        );

        return Ok(new { message = $"Role alterado de {antes} para {role}" });
    }
}
