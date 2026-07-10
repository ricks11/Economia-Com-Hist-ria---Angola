using EconomiaComHistoria.Core.DTOs;
using EconomiaComHistoria.Infrastructure.Data;
using EconomiaComHistoria.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECHA.API.Controllers;

[ApiController]
[Route("api/escolas")]
[Authorize(Roles = "Admin")]
public class EscolasController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IEscolaService _escolaService;

    public EscolasController(AppDbContext context, IEscolaService escolaService)
    {
        _context = context;
        _escolaService = escolaService;
    }

    [HttpGet]
    public async Task<ActionResult<List<EscolaResponseDto>>> GetEscolas(CancellationToken ct)
    {
        var escolas = await _escolaService.ListarEscolasAsync(ct);
        return Ok(escolas);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<EscolaResponseDto>> GetEscola(int id, CancellationToken ct)
    {
        var escola = await _context.Escolas
            .Include(e => e.Turmas)
            .FirstOrDefaultAsync(e => e.Id == id, ct);
        
        if (escola == null) return NotFound();

        return Ok(new EscolaResponseDto(
            escola.Id, 
            escola.Nome, 
            null, 
            escola.Provincia, 
            escola.Municipio, 
            null, 
            null, 
            escola.Turmas.Sum(t => t.Alunos.Count), 
            escola.Turmas.Count
        ));
    }

    [HttpPost]
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
    public async Task<IActionResult> DeleteEscola(int id, CancellationToken ct)
    {
        var escola = await _context.Escolas.FindAsync(new object[] { id }, ct);
        if (escola == null) return NotFound();

        _context.Escolas.Remove(escola);
        await _context.SaveChangesAsync(ct);

        return NoContent();
    }

    [HttpPost("{id}/convite")]
    public async Task<ActionResult<InviteCodeResponseDto>> GerarConvite(int id, CancellationToken ct)
    {
        var invite = await _escolaService.GerarCodigoConviteAsync(id, 7, ct);
        return Ok(invite);
    }
}
