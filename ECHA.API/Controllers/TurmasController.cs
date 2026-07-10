using EconomiaComHistoria.Core.DTOs;
using EconomiaComHistoria.Core.Entities;
using EconomiaComHistoria.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ECHA.API.Controllers;

[ApiController]
[Route("api/turmas")]
[Authorize]
public class TurmasController : ControllerBase
{
    private readonly AppDbContext _context;

    public TurmasController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<List<TurmaResponseDto>>> GetTurmas([FromQuery] int? escolaId, CancellationToken ct)
    {
        var query = _context.Turmas.Include(t => t.Escola).Include(t => t.Professor).AsQueryable();

        if (escolaId.HasValue)
            query = query.Where(t => t.EscolaId == escolaId.Value);

        var turmas = await query.ToListAsync(ct);
        var response = turmas.Select(t => new TurmaResponseDto(
            t.Id, t.Nome, ParseAno(t.Ano), t.EscolaId, t.Escola?.Nome, t.ProfessorId, t.Professor?.Nome, t.Alunos.Count
        )).ToList();

        return Ok(response);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TurmaDetalheDto>> GetTurma(int id, CancellationToken ct)
    {
        var turma = await _context.Turmas
            .Include(t => t.Escola)
            .Include(t => t.Professor)
            .Include(t => t.Alunos)
            .FirstOrDefaultAsync(t => t.Id == id, ct);

        if (turma == null) return NotFound();

        var alunos = turma.Alunos.Select(a => new AlunoResumoDto(a.Id, a.Nome, a.Email, a.PontosTotais)).ToList();

        return Ok(new TurmaDetalheDto(
            turma.Id, turma.Nome, ParseAno(turma.Ano), turma.EscolaId, turma.Escola?.Nome, turma.ProfessorId, turma.Professor?.Nome, alunos
        ));
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Editor")]
    public async Task<ActionResult<TurmaResponseDto>> CreateTurma([FromBody] CreateTurmaDto dto, CancellationToken ct)
    {
        var turma = new Turma
        {
            Nome = dto.Nome,
            Ano = dto.Ano?.ToString() ?? string.Empty,
            EscolaId = dto.EscolaId,
            ProfessorId = dto.ProfessorId
        };

        _context.Turmas.Add(turma);
        await _context.SaveChangesAsync(ct);

        return CreatedAtAction(nameof(GetTurma), new { id = turma.Id }, new TurmaResponseDto(
            turma.Id, turma.Nome, ParseAno(turma.Ano), turma.EscolaId, null, turma.ProfessorId, null, 0
        ));
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Editor")]
    public async Task<ActionResult<TurmaResponseDto>> UpdateTurma(int id, [FromBody] UpdateTurmaDto dto, CancellationToken ct)
    {
        var turma = await _context.Turmas.FindAsync(new object[] { id }, ct);
        if (turma == null) return NotFound();

        turma.Nome = dto.Nome;
        turma.Ano = dto.Ano?.ToString() ?? string.Empty;
        turma.ProfessorId = dto.ProfessorId;

        await _context.SaveChangesAsync(ct);

        return Ok(new TurmaResponseDto(
            turma.Id, turma.Nome, ParseAno(turma.Ano), turma.EscolaId, null, turma.ProfessorId, null, turma.Alunos.Count
        ));
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,Editor")]
    public async Task<IActionResult> DeleteTurma(int id, CancellationToken ct)
    {
        var turma = await _context.Turmas.FindAsync(new object[] { id }, ct);
        if (turma == null) return NotFound();

        _context.Turmas.Remove(turma);
        await _context.SaveChangesAsync(ct);

        return NoContent();
    }

    [HttpPost("{id}/alunos")]
    [Authorize(Roles = "Admin,Editor")]
    public async Task<ActionResult> AdicionarAluno(int id, [FromBody] int alunoId, CancellationToken ct)
    {
        var turma = await _context.Turmas.Include(t => t.Alunos).FirstOrDefaultAsync(t => t.Id == id, ct);
        if (turma == null) return NotFound();

        var aluno = await _context.Utilizadores.FindAsync(new object[] { alunoId }, ct);
        if (aluno == null) return BadRequest("Aluno não encontrado");

        if (!turma.Alunos.Any(a => a.Id == alunoId))
        {
            turma.Alunos.Add(aluno);
            aluno.TurmaId = id;
            await _context.SaveChangesAsync(ct);
        }

        return NoContent();
    }

    private static int? ParseAno(string? ano)
    {
        return int.TryParse(ano, out var value) ? value : null;
    }
}
