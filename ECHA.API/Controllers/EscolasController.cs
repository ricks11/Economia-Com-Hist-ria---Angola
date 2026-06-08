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

        return CreatedAtAction(nameof(GetEscolas), new { id = escola.Id }, new EscolaResponseDto(
            escola.Id, escola.Nome, null, escola.Provincia, escola.Municipio, null, null, 0, 0));
    }

    [HttpPost("{id}/convite")]
    public async Task<ActionResult<InviteCodeResponseDto>> GerarConvite(int id, CancellationToken ct)
    {
        var invite = await _escolaService.GerarCodigoConviteAsync(id, 7, ct);
        return Ok(invite);
    }
}
