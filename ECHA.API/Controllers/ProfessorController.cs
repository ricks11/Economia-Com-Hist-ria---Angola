using EconomiaComHistoria.Core.DTOs;
using EconomiaComHistoria.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ECHA.API.Controllers;

/// <summary>
/// Dashboard do Professor: estatísticas agregadas das turmas e alunos.
/// </summary>
[ApiController]
[Route("api/professor")]
[Authorize(Roles = "Professor,Editor,Admin,SuperAdmin")]
public class ProfessorController : ControllerBase
{
    private readonly AppDbContext _db;

    public ProfessorController(AppDbContext db) => _db = db;

    private bool TryGetUserId(out int userId)
    {
        var val = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
               ?? User.FindFirst("sub")?.Value;
        return int.TryParse(val, out userId);
    }

    /// <summary>
    /// GET api/professor/dashboard
    /// Retorna as estatísticas consolidadas do professor autenticado.
    /// </summary>
    [HttpGet("dashboard")]
    [ProducesResponseType(typeof(ProfessorDashboardDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ProfessorDashboardDto>> GetDashboard(CancellationToken ct)
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized(new { message = "Utilizador não autenticado." });

        // Turmas do professor
        var turmas = await _db.Turmas
            .Where(t => t.ProfessorId == userId)
            .Include(t => t.Alunos)
            .AsNoTracking()
            .ToListAsync(ct);

        var turmaResumos = turmas.Select(t => new TurmaResumoDto(
            t.Id,
            t.Nome,
            t.Alunos.Count,
            t.Alunos.Any() ? t.Alunos.Average(a => a.PontosTotais) : 0.0,
            t.Ano
        )).ToList();

        // Total de alunos únicos
        var totalAlunos = turmas.SelectMany(t => t.Alunos).Select(a => a.Id).Distinct().Count();

        // Quizzes ativos (qualquer quiz marcado como ativo)
        var quizzesAtivos = await _db.Quizzes.CountAsync(q => q.Ativo, ct);

        // Média de pontos em todas as turmas
        var mediaPontos = turmaResumos.Any() ? turmaResumos.Average(t => t.MediaPontos) : 0.0;

        // Alunos com atividade recente (últimos 7 dias)
        var dataLimite = DateTime.UtcNow.AddDays(-7);
        var alunosIds = turmas.SelectMany(t => t.Alunos).Select(a => a.Id).Distinct().ToList();

        var alunosRecentes = await _db.Utilizadores
            .Where(u => alunosIds.Contains(u.Id))
            .OrderByDescending(u => u.PontosTotais)
            .Take(10)
            .Select(u => new AlunoAtividadeRecenteDto(
                u.Id,
                u.Nome,
                u.PontosTotais,
                null // UltimaAtividade será implementada com tabela de auditoria futura
            ))
            .AsNoTracking()
            .ToListAsync(ct);

        var dto = new ProfessorDashboardDto(
            totalAlunos,
            turmas.Count,
            quizzesAtivos,
            mediaPontos,
            turmaResumos,
            alunosRecentes
        );

        return Ok(dto);
    }
}
