using EconomiaComHistoria.Core.DTOs;
using EconomiaComHistoria.Core.Entities;
using EconomiaComHistoria.Core.Enums;
using EconomiaComHistoria.Core.Interfaces;
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
    private readonly IAuditoriaService _auditoriaService;

    public TurmasController(AppDbContext context, IAuditoriaService auditoriaService)
    {
        _context = context;
        _auditoriaService = auditoriaService;
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

        var userId = TryGetUserId(out var uid) ? uid : 0;
        await _auditoriaService.RegistarAsync(
            userId,
            "CriarTurma",
            "Turma",
            turma.Id,
            null,
            $"Nome: {turma.Nome}, EscolaId: {turma.EscolaId}",
            HttpContext
        );

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

        var antes = System.Text.Json.JsonSerializer.Serialize(new { turma.Nome, turma.ProfessorId });

        turma.Nome = dto.Nome;
        turma.Ano = dto.Ano?.ToString() ?? string.Empty;
        turma.ProfessorId = dto.ProfessorId;

        await _context.SaveChangesAsync(ct);

        var depois = System.Text.Json.JsonSerializer.Serialize(new { turma.Nome, turma.ProfessorId });
        await _auditoriaService.RegistarAsync(
            id,
            "AtualizarTurma",
            "Turma",
            id,
            antes,
            depois,
            HttpContext
        );

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

        await _auditoriaService.RegistarAsync(
            id,
            "EliminarTurma",
            "Turma",
            id,
            null,
            "Eliminada",
            HttpContext
        );

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

    /// <summary>
    /// GET api/turmas/{id}/ranking
    /// Retorna o ranking dos alunos de uma turma específica.
    /// </summary>
    [HttpGet("{id}/ranking")]
    [ProducesResponseType(typeof(TurmaRankingResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<TurmaRankingResponseDto>> GetRankingTurma(int id, CancellationToken ct)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();

        var turma = await _context.Turmas
            .Include(t => t.Alunos)
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id, ct);

        if (turma == null) return NotFound(new { message = "Turma não encontrada." });

        var entradas = turma.Alunos
            .OrderByDescending(a => a.PontosTotais)
            .Select((a, index) => new TurmaRankingEntradaDto(
                index + 1,
                a.Id,
                a.Nome,
                a.PontosTotais,
                0, // QuizzesCompletados — será enriquecido com tabela de tentativas futura
                a.Id == userId
            ))
            .ToList();

        var posicaoUtilizador = entradas.FirstOrDefault(e => e.IsCurrentUser)?.Posicao ?? 0;

        return Ok(new TurmaRankingResponseDto(turma.Id, turma.Nome, entradas, posicaoUtilizador));
    }

    private bool TryGetUserId(out int userId)
    {
        var value = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                 ?? User.FindFirst("sub")?.Value;
        return int.TryParse(value, out userId);
    }
}