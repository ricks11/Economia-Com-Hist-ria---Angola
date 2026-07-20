using EconomiaComHistoria.Core.DTOs;
using EconomiaComHistoria.Core.Entities;
using EconomiaComHistoria.Core.Enums;
using EconomiaComHistoria.Core.Interfaces;
using EconomiaComHistoria.Infrastructure.Data;
using EconomiaComHistoria.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ECHA.API.Controllers;

[ApiController]
[Route("api/escolas")]
[Authorize]
public class EscolasController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IEscolaService _escolaService;
    private readonly IAuditoriaService _auditoriaService;

    public EscolasController(AppDbContext context, IEscolaService escolaService, IAuditoriaService auditoriaService )
    {
        _context = context;
        _escolaService = escolaService;
        _auditoriaService = auditoriaService;
    }

    private bool TryGetUserId(out int userId)
    {
        var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? User.FindFirst("sub")?.Value;
        return int.TryParse(userIdStr, out userId);
    }

    private async Task<Utilizador?> GetCurrentUserAsync(CancellationToken ct)
    {
        if (!TryGetUserId(out var userId))
            return null;
        return await _context.Utilizadores
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId, ct);
    }

    private bool IsAdmin(TipoUtilizador tipo) =>
        tipo == TipoUtilizador.Admin || tipo == TipoUtilizador.SuperAdmin;

    [HttpGet]
    public async Task<ActionResult<List<EscolaResponseDto>>> GetEscolas(CancellationToken ct)
    {
        var currentUser = await GetCurrentUserAsync(ct);
        if (currentUser == null)
            return Unauthorized();

        var query = _context.Escolas
            .Include(e => e.Turmas)
                .ThenInclude(t => t.Alunos)
            .AsNoTracking();

        if (!IsAdmin(currentUser.Tipo))
        {
            if (currentUser.EscolaId.HasValue)
                query = query.Where(e => e.Id == currentUser.EscolaId.Value);
            else
                return Ok(new List<EscolaResponseDto>());
        }

        var escolas = await query.ToListAsync(ct);

        return Ok(escolas.Select(e => new EscolaResponseDto(
            e.Id,
            e.Nome,
            e.CodigoMEC,
            e.Provincia,
            e.Municipio,
            e.CodigoConvite,
            e.CodigoConviteExpiracao,
            e.Turmas.Sum(t => t.Alunos.Count),
            e.Turmas.Count
        )));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<EscolaResponseDto>> GetEscola(int id, CancellationToken ct)
    {
        var currentUser = await GetCurrentUserAsync(ct);
        if (currentUser == null)
            return Unauthorized();

        var escola = await _context.Escolas
            .Include(e => e.Turmas)
                .ThenInclude(t => t.Alunos)
            .FirstOrDefaultAsync(e => e.Id == id, ct);

        if (escola == null)
            return NotFound();

        if (!IsAdmin(currentUser.Tipo) && (currentUser.EscolaId == null || currentUser.EscolaId != id))
            return Forbid();

        return Ok(new EscolaResponseDto(
            escola.Id,
            escola.Nome,
            escola.CodigoMEC,
            escola.Provincia,
            escola.Municipio,
            escola.CodigoConvite,
            escola.CodigoConviteExpiracao,
            escola.Turmas.Sum(t => t.Alunos.Count),
            escola.Turmas.Count
        ));
    }

    [HttpPost]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<ActionResult<EscolaResponseDto>> CreateEscola([FromBody] CreateEscolaDto dto, CancellationToken ct)
    {
        var escola = new Escola
        {
            Nome = dto.Nome,
            CodigoMEC = dto.CodigoMEC,
            Provincia = dto.Provincia ?? string.Empty,
            Municipio = dto.Localizacao
        };

        _context.Escolas.Add(escola);
        await _context.SaveChangesAsync(ct);

        // Gerar código de convite inicial
        var invite = await _escolaService.GerarCodigoConviteAsync(escola.Id, 7, ct);
        escola.CodigoConvite = invite.Codigo;
        escola.CodigoConviteExpiracao = invite.ExpiraEm;
        await _context.SaveChangesAsync(ct);

        var userId = TryGetUserId(out var uid) ? uid : 0;
        await _auditoriaService.RegistarAsync(
            userId,
            "CriarEscola",
            "Escola",
            escola.Id,
            null,
            $"Nome: {escola.Nome}",
            HttpContext
        );

        return CreatedAtAction(nameof(GetEscola), new { id = escola.Id }, new EscolaResponseDto(
            escola.Id, escola.Nome, escola.CodigoMEC, escola.Provincia, escola.Municipio,
            escola.CodigoConvite, escola.CodigoConviteExpiracao, 0, 0));
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<ActionResult<EscolaResponseDto>> UpdateEscola(int id, [FromBody] CreateEscolaDto dto, CancellationToken ct)
    {
        var escola = await _context.Escolas.FindAsync(new object[] { id }, ct);
        if (escola == null) return NotFound();

        var antes = System.Text.Json.JsonSerializer.Serialize(new { escola.Nome, escola.Provincia, escola.Municipio });

        escola.Nome = dto.Nome;
        escola.CodigoMEC = dto.CodigoMEC;
        escola.Provincia = dto.Provincia ?? string.Empty;
        escola.Municipio = dto.Localizacao;

        await _context.SaveChangesAsync(ct);

        var depois = System.Text.Json.JsonSerializer.Serialize(new { escola.Nome, escola.Provincia, escola.Municipio });
        await _auditoriaService.RegistarAsync(
            id,
            "AtualizarEscola",
            "Escola",
            id,
            antes,
            depois,
            HttpContext
        );

        var updated = await _context.Escolas
            .Include(e => e.Turmas)
                .ThenInclude(t => t.Alunos)
            .FirstAsync(e => e.Id == id, ct);

        return Ok(new EscolaResponseDto(
            updated.Id, updated.Nome, updated.CodigoMEC, updated.Provincia, updated.Municipio,
            updated.CodigoConvite, updated.CodigoConviteExpiracao,
            updated.Turmas.Sum(t => t.Alunos.Count),
            updated.Turmas.Count
        ));
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> DeleteEscola(int id, CancellationToken ct)
    {
        var escola = await _context.Escolas.FindAsync(new object[] { id }, ct);
        if (escola == null) return NotFound();

        _context.Escolas.Remove(escola);
        await _context.SaveChangesAsync(ct);

        await _auditoriaService.RegistarAsync(
            id,
            "EliminarEscola",
            "Escola",
            id,
            null,
            "Eliminada",
            HttpContext
        );

        return NoContent();
    }

    [HttpPost("{id}/convite")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<ActionResult<InviteCodeResponseDto>> GerarConvite(int id, CancellationToken ct)
    {
        var escola = await _context.Escolas.FindAsync(new object[] { id }, ct);
        if (escola == null) return NotFound();

        var invite = await _escolaService.GerarCodigoConviteAsync(id, 7, ct);
        escola.CodigoConvite = invite.Codigo;
        escola.CodigoConviteExpiracao = invite.ExpiraEm;
        await _context.SaveChangesAsync(ct);

        await _auditoriaService.RegistarAsync(
            id,
            "GerarConvite",
            "Escola",
            id,
            null,
            $"Código: {invite.Codigo}",
            HttpContext
        );

        return Ok(invite);
    }

    [HttpDelete("{id}/convite")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> RevogarConvite(int id, CancellationToken ct)
    {
        var escola = await _context.Escolas.FindAsync(new object[] { id }, ct);
        if (escola == null) return NotFound();

        escola.CodigoConvite = null;
        escola.CodigoConviteExpiracao = null;
        await _context.SaveChangesAsync(ct);

        await _auditoriaService.RegistarAsync(
            id,
            "RevogarConvite",
            "Escola",
            id,
            null,
            "Código revogado",
            HttpContext
        );

        return NoContent();
    }
}