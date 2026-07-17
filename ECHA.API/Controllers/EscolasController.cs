using EconomiaComHistoria.Core.DTOs;
using EconomiaComHistoria.Core.Entities;
using EconomiaComHistoria.Core.Enums;
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

    public EscolasController(AppDbContext context, IEscolaService escolaService)
    {
        _context = context;
        _escolaService = escolaService;
    }

    // Método auxiliar para obter o ID do utilizador autenticado
    private bool TryGetUserId(out int userId)
    {
        var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? User.FindFirst("sub")?.Value;
        return int.TryParse(userIdStr, out userId);
    }

    // Método auxiliar para obter o utilizador atual (incluindo EscolaId e Tipo)
    private async Task<Utilizador?> GetCurrentUserAsync(CancellationToken ct)
    {
        if (!TryGetUserId(out var userId))
            return null;

        return await _context.Utilizadores
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId, ct);
    }

    [HttpGet]
    public async Task<ActionResult<List<EscolaResponseDto>>> GetEscolas(CancellationToken ct)
    {
        var currentUser = await GetCurrentUserAsync(ct);
        if (currentUser == null)
            return Unauthorized(new { message = "Utilizador não autenticado" });

        // 1. Buscar todas as escolas (através do serviço existente)
        var todasEscolas = await _escolaService.ListarEscolasAsync(ct);

        // 2. Aplicar filtros baseados no tipo de utilizador
        var isAdmin = currentUser.Tipo == TipoUtilizador.Admin
                   || currentUser.Tipo == TipoUtilizador.SuperAdmin;

        if (isAdmin)
        {
            // Admin e SuperAdmin veem todas as escolas
            return Ok(todasEscolas);
        }

        // Para outros utilizadores (Estudante, Professor, Editor, Moderador, ClienteInstitucional)
        // - Se tiver escola associada, vê apenas essa escola
        // - Caso contrário, lista vazia
        if (currentUser.EscolaId.HasValue)
        {
            var escolaDoUtilizador = todasEscolas
                .Where(e => e.Id == currentUser.EscolaId.Value)
                .ToList();
            return Ok(escolaDoUtilizador);
        }

        // Utilizador sem escola → lista vazia
        return Ok(new List<EscolaResponseDto>());
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<EscolaResponseDto>> GetEscola(int id, CancellationToken ct)
    {
        var currentUser = await GetCurrentUserAsync(ct);
        if (currentUser == null)
            return Unauthorized();

        // Buscar a escola com detalhes (incluindo turmas)
        var escola = await _context.Escolas
            .Include(e => e.Turmas)
            .ThenInclude(t => t.Alunos)  // Para contar alunos
            .FirstOrDefaultAsync(e => e.Id == id, ct);

        if (escola == null)
            return NotFound();

        // Verificar permissão
        var isAdmin = currentUser.Tipo == TipoUtilizador.Admin
                   || currentUser.Tipo == TipoUtilizador.SuperAdmin;

        if (!isAdmin)
        {
            // Utilizador comum só pode ver a sua própria escola
            if (currentUser.EscolaId == null || currentUser.EscolaId != id)
                return Forbid(); // Ou NotFound() para não expor existência
        }

        // Mapear para DTO
        var response = new EscolaResponseDto(
            escola.Id,
            escola.Nome,
            null, // Imagem (não usada neste exemplo)
            escola.Provincia,
            escola.Municipio,
            null, // Contacto
            null, // Email
            escola.Turmas.Sum(t => t.Alunos.Count),
            escola.Turmas.Count
        );

        return Ok(response);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<ActionResult<EscolaResponseDto>> CreateEscola([FromBody] CreateEscolaDto dto, CancellationToken ct)
    {
        var escola = new EconomiaComHistoria.Core.Entities.Escola
        {
            Nome = dto.Nome,
            Provincia = dto.Provincia ?? string.Empty,
            Municipio = dto.Localizacao
        };

        _context.Escolas.Add(escola);
        await _context.SaveChangesAsync(ct);

        return CreatedAtAction(nameof(GetEscola), new { id = escola.Id }, new EscolaResponseDto(
            escola.Id, escola.Nome, null, escola.Provincia, escola.Municipio, null, null, 0, 0));
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<ActionResult<EscolaResponseDto>> UpdateEscola(int id, [FromBody] CreateEscolaDto dto, CancellationToken ct)
    {
        var escola = await _context.Escolas.FindAsync(new object[] { id }, ct);
        if (escola == null) return NotFound();

        escola.Nome = dto.Nome;
        escola.Provincia = dto.Provincia ?? string.Empty;
        escola.Municipio = dto.Localizacao;

        await _context.SaveChangesAsync(ct);

        return Ok(new EscolaResponseDto(
            escola.Id, escola.Nome, null, escola.Provincia, escola.Municipio, null, null, 0, 0
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

        return NoContent();
    }

    [HttpPost("{id}/convite")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<ActionResult<InviteCodeResponseDto>> GerarConvite(int id, CancellationToken ct)
    {
        var invite = await _escolaService.GerarCodigoConviteAsync(id, 7, ct);
        return Ok(invite);
    }
}