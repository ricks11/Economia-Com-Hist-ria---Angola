using EconomiaComHistoria.Core.DTOs;
using EconomiaComHistoria.Core.Entities;
using EconomiaComHistoria.Core.Enums;   // Certifique-se de que o enum TipoUtilizador está acessível
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
    public async Task<ActionResult<List<TurmaResponseDto>>> GetTurmas(
        [FromQuery] int? escolaId,
        CancellationToken ct)
    {
        // 1. Obter utilizador autenticado
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                       ?? User.FindFirst("sub")?.Value;
        if (!int.TryParse(userIdClaim, out var userId))
            return Unauthorized(new { message = "Utilizador não autenticado" });

        var user = await _context.Utilizadores
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId, ct);

        if (user == null)
            return Unauthorized(new { message = "Utilizador não encontrado" });

        // 2. Construir query base
        var query = _context.Turmas
            .Include(t => t.Escola)
            .Include(t => t.Professor)
            .AsQueryable();

        // 3. Filtrar por escolaId se fornecido
        if (escolaId.HasValue)
            query = query.Where(t => t.EscolaId == escolaId.Value);

        // 4. Aplicar regras de permissão
        bool isAdmin = user.Tipo == TipoUtilizador.Admin;
        bool isCliente = user.Tipo == TipoUtilizador.ClienteInstitucional;

        if (!isAdmin && !isCliente)
        {
            // Utilizadores comuns (Estudante, Professor, Editor, Moderador)
            // Só podem ver turmas da sua própria escola
            if (user.EscolaId == null)
                return Ok(new List<TurmaResponseDto>());   // Sem escola associada → sem turmas

            query = query.Where(t => t.EscolaId == user.EscolaId.Value);
        }
        // Para Admin e Cliente: mantêm-se todas (ou filtradas por escolaId se passado)

        // 5. Executar e mapear
        var turmas = await query.ToListAsync(ct);
        var response = turmas.Select(t => new TurmaResponseDto(
            t.Id,
            t.Nome,
            ParseAno(t.Ano),
            t.EscolaId,
            t.Escola?.Nome,
            t.ProfessorId,
            t.Professor?.Nome,
            t.Alunos.Count
        )).ToList();

        return Ok(response);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TurmaDetalheDto>> GetTurma(int id, CancellationToken ct)
    {
        // (Opcional) Aplicar a mesma lógica de permissão para detalhe de uma turma específica
        // Pode ser adicionado se necessário, mas o foco é a listagem.

        var turma = await _context.Turmas
            .Include(t => t.Escola)
            .Include(t => t.Professor)
            .Include(t => t.Alunos)
            .FirstOrDefaultAsync(t => t.Id == id, ct);

        if (turma == null)
            return NotFound();

        // Verificar se o utilizador tem permissão para ver esta turma
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
        if (!int.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var user = await _context.Utilizadores.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId, ct);
        if (user == null)
            return Unauthorized();

        bool isAdmin = user.Tipo == TipoUtilizador.Admin;
        bool isCliente = user.Tipo == TipoUtilizador.ClienteInstitucional;

        if (!isAdmin && !isCliente)
        {
            // Utilizador comum: só pode ver se a turma for da sua escola
            if (user.EscolaId == null || turma.EscolaId != user.EscolaId)
                return Forbid();   // Ou NotFound() para não expor existência
        }

        var alunos = turma.Alunos.Select(a => new AlunoResumoDto(a.Id, a.Nome, a.Email, a.PontosTotais)).ToList();

        return Ok(new TurmaDetalheDto(
            turma.Id,
            turma.Nome,
            ParseAno(turma.Ano),
            turma.EscolaId,
            turma.Escola?.Nome,
            turma.ProfessorId,
            turma.Professor?.Nome,
            alunos
        ));
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Editor,SuperAdmin")]
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
    [Authorize(Roles = "Admin,Editor,SuperAdmin")]
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
    [Authorize(Roles = "Admin,Editor,SuperAdmin")]
    public async Task<IActionResult> DeleteTurma(int id, CancellationToken ct)
    {
        var turma = await _context.Turmas.FindAsync(new object[] { id }, ct);
        if (turma == null) return NotFound();

        _context.Turmas.Remove(turma);
        await _context.SaveChangesAsync(ct);

        return NoContent();
    }

    [HttpPost("{id}/alunos")]
    [Authorize(Roles = "Admin,Editor,SuperAdmin")]
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