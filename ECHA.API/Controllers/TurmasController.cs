using EconomiaComHistoria.Core.DTOs;
using EconomiaComHistoria.Core.Entities;
using EconomiaComHistoria.Core.Enums;
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

    // Helper para obter utilizador atual
    private async Task<Utilizador?> GetCurrentUserAsync(CancellationToken ct)
    {
        var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
        if (!int.TryParse(userIdStr, out var userId)) return null;
        return await _context.Utilizadores.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId, ct);
    }

    private bool IsAdminOrCliente(TipoUtilizador tipo) =>
        tipo == TipoUtilizador.Admin || tipo == TipoUtilizador.SuperAdmin || tipo == TipoUtilizador.ClienteInstitucional;

    [HttpGet]
    public async Task<ActionResult<List<TurmaResponseDto>>> GetTurmas(
        [FromQuery] int? escolaId,
        CancellationToken ct)
    {
        var user = await GetCurrentUserAsync(ct);
        if (user == null) return Unauthorized();

        var query = _context.Turmas
            .Include(t => t.Escola)
            .Include(t => t.Professor)
            .Include(t => t.Alunos)
            .AsQueryable();

        if (escolaId.HasValue)
            query = query.Where(t => t.EscolaId == escolaId.Value);

        // Aplicar permissões
        if (!IsAdminOrCliente(user.Tipo))
        {
            // Utilizador comum (Estudante, Professor, Editor, Moderador)
            if (user.EscolaId == null)
                return Ok(new List<TurmaResponseDto>());

            query = query.Where(t => t.EscolaId == user.EscolaId.Value);
        }
        // Admin/Cliente veem todas (ou filtradas por escolaId se passado)

        var turmas = await query.ToListAsync(ct);
        return Ok(turmas.Select(t => new TurmaResponseDto(
            t.Id,
            t.Nome,
            string.IsNullOrEmpty(t.Ano) ? null : int.Parse(t.Ano),
            t.EscolaId,
            t.Escola?.Nome,
            t.ProfessorId,
            t.Professor?.Nome,
            t.Alunos.Count
        )));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TurmaDetalheDto>> GetTurma(int id, CancellationToken ct)
    {
        var user = await GetCurrentUserAsync(ct);
        if (user == null) return Unauthorized();

        var turma = await _context.Turmas
            .Include(t => t.Escola)
            .Include(t => t.Professor)
            .Include(t => t.Alunos)
            .FirstOrDefaultAsync(t => t.Id == id, ct);

        if (turma == null) return NotFound();

        // Verificar permissão
        if (!IsAdminOrCliente(user.Tipo) && (user.EscolaId == null || turma.EscolaId != user.EscolaId))
            return Forbid();

        return Ok(new TurmaDetalheDto(
            turma.Id,
            turma.Nome,
            string.IsNullOrEmpty(turma.Ano) ? null : int.Parse(turma.Ano),
            turma.EscolaId,
            turma.Escola?.Nome,
            turma.ProfessorId,
            turma.Professor?.Nome,
            turma.Alunos.Select(a => new AlunoResumoDto(a.Id, a.Nome, a.Email, a.PontosTotais)).ToList()
        ));
    }

    [HttpPost]
    [Authorize(Roles = "Admin,SuperAdmin,Editor")]
    [HttpPost]
    [Authorize(Roles = "Admin,SuperAdmin,Editor")]
    public async Task<ActionResult<TurmaResponseDto>> CreateTurma([FromBody] CreateTurmaDto dto, CancellationToken ct)
    {
        // Validar se a Escola existe
        var escola = await _context.Escolas.FindAsync(new object[] { dto.EscolaId }, ct);
        if (escola == null)
            return BadRequest(new { message = "Escola não encontrada." });

        // Validar se o Professor existe e tem a role correta
        var professor = await _context.Utilizadores
            .FirstOrDefaultAsync(u => u.Id == dto.ProfessorId && u.Tipo == TipoUtilizador.Professor, ct);
        if (professor == null)
            return BadRequest(new { message = "Professor não encontrado ou não possui a role 'Professor'." });

        var turma = new Turma
        {
            Nome = dto.Nome,
            Ano = dto.Ano?.ToString() ?? string.Empty,
            EscolaId = dto.EscolaId,
            ProfessorId = dto.ProfessorId,
            Ativa = true
        };

        _context.Turmas.Add(turma);
        await _context.SaveChangesAsync(ct);

        var created = await _context.Turmas
            .Include(t => t.Escola)
            .Include(t => t.Professor)
            .FirstAsync(t => t.Id == turma.Id, ct);

        return CreatedAtAction(nameof(GetTurma), new { id = turma.Id }, new TurmaResponseDto(
            turma.Id,
            turma.Nome,
            string.IsNullOrEmpty(turma.Ano) ? null : int.Parse(turma.Ano),
            turma.EscolaId,
            turma.Escola?.Nome,
            turma.ProfessorId,
            turma.Professor?.Nome,
            0
        ));
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,SuperAdmin,Editor")]
    public async Task<ActionResult<TurmaResponseDto>> UpdateTurma(int id, [FromBody] UpdateTurmaDto dto, CancellationToken ct)
    {
        var turma = await _context.Turmas
            .Include(t => t.Escola)
            .Include(t => t.Professor)
            .Include(t => t.Alunos)
            .FirstOrDefaultAsync(t => t.Id == id, ct);

        if (turma == null) return NotFound();

        turma.Nome = dto.Nome;
        turma.Ano = dto.Ano?.ToString() ?? string.Empty;
        turma.ProfessorId = dto.ProfessorId;

        await _context.SaveChangesAsync(ct);

        return Ok(new TurmaResponseDto(
            turma.Id,
            turma.Nome,
            string.IsNullOrEmpty(turma.Ano) ? null : int.Parse(turma.Ano),
            turma.EscolaId,
            turma.Escola?.Nome,
            turma.ProfessorId,
            turma.Professor?.Nome,
            turma.Alunos.Count
        ));
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,SuperAdmin,Editor")]
    public async Task<IActionResult> DeleteTurma(int id, CancellationToken ct)
    {
        var turma = await _context.Turmas.FindAsync(new object[] { id }, ct);
        if (turma == null) return NotFound();

        _context.Turmas.Remove(turma);
        await _context.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpPost("{id}/alunos")]
    [Authorize(Roles = "Admin,SuperAdmin,Editor")]
    public async Task<IActionResult> AdicionarAluno(int id, [FromBody] int alunoId, CancellationToken ct)
    {
        var turma = await _context.Turmas.Include(t => t.Alunos).FirstOrDefaultAsync(t => t.Id == id, ct);
        if (turma == null) return NotFound();

        var aluno = await _context.Utilizadores.FindAsync(new object[] { alunoId }, ct);
        if (aluno == null) return BadRequest(new { message = "Aluno não encontrado" });

        if (turma.Alunos.Any(a => a.Id == alunoId))
            return BadRequest(new { message = "Aluno já está nesta turma" });

        turma.Alunos.Add(aluno);
        aluno.TurmaId = id;
        await _context.SaveChangesAsync(ct);

        return NoContent();
    }

    [HttpDelete("{id}/alunos/{alunoId}")]
    [Authorize(Roles = "Admin,SuperAdmin,Editor")]
    public async Task<IActionResult> RemoverAluno(int id, int alunoId, CancellationToken ct)
    {
        var turma = await _context.Turmas.Include(t => t.Alunos).FirstOrDefaultAsync(t => t.Id == id, ct);
        if (turma == null) return NotFound();

        var aluno = turma.Alunos.FirstOrDefault(a => a.Id == alunoId);
        if (aluno == null) return BadRequest(new { message = "Aluno não está nesta turma" });

        turma.Alunos.Remove(aluno);
        aluno.TurmaId = null;
        await _context.SaveChangesAsync(ct);

        return NoContent();
    }
}