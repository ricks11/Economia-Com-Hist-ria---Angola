using EconomiaComHistoria.Core.DTOs;
using EconomiaComHistoria.Core.Helpers;
using EconomiaComHistoria.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECHA.API.Controllers;

[ApiController]
[Route("api/perfil")]
[Authorize]
public class PerfilController : ControllerBase
{
    private readonly AppDbContext _dbContext;

    public PerfilController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet("progresso")]
    public async Task<ActionResult<ProgressoUtilizadorDto>> GetProgresso(CancellationToken ct)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out var userId)) return Unauthorized();

        var user = await _dbContext.Utilizadores
            .Include(u => u.BadgesConquistados)
            .ThenInclude(bc => bc.Badge)
            .FirstOrDefaultAsync(u => u.Id == userId, ct);

        if (user == null) return NotFound();

        // Lógica simples de nível: cada 1000 pontos = 1 nível
        int nivel = (user.PontosTotais / 1000) + 1;
        int pontosNoNivelAtual = user.PontosTotais % 1000;
        double percentagemNivel = (double)pontosNoNivelAtual / 1000 * 100;

        var badges = user.BadgesConquistados.Select(bc => new BadgeConquistadoDto(
            bc.BadgeId,
            bc.Badge?.Nome ?? "Badge",
            bc.Badge?.Descricao,
            bc.Badge?.Icone,
            bc.DataConquista
        )).ToList();

        var response = new ProgressoUtilizadorDto(
            user.PontosTotais,
            nivel,
            1000 - pontosNoNivelAtual,
            percentagemNivel,
            user.StreakAtual,
            badges
        );

        return Ok(response);
    }

    /// <summary>
    /// Gets the current authenticated user's profile
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PerfilResponseDto>> GetPerfil(CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirst("sub")?.Value;
        if (!int.TryParse(userIdClaim, out var userId))
            return Unauthorized(new { message = "Utilizador não autenticado" });

        var utilizador = await _dbContext.Utilizadores
            .Include(u => u.Escola)
            .Include(u => u.Turma)
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (utilizador is null)
            return NotFound(new { message = "Utilizador não encontrado" });

        var response = MapToPerfilResponseDto(utilizador);
        return Ok(response);
    }

    /// <summary>
    /// Updates the current authenticated user's profile
    /// </summary>
    [HttpPut]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PerfilResponseDto>> UpdatePerfil(
        [FromBody] UpdatePerfilDto request,
        CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirst("sub")?.Value;
        if (!int.TryParse(userIdClaim, out var userId))
            return Unauthorized(new { message = "Utilizador não autenticado" });

        var utilizador = await _dbContext.Utilizadores
            .Include(u => u.Escola)
            .Include(u => u.Turma)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (utilizador is null)
            return NotFound(new { message = "Utilizador não encontrado" });

        // Update optional fields
        if (!string.IsNullOrEmpty(request.Nome))
            utilizador.Nome = request.Nome;

        if (!string.IsNullOrEmpty(request.Provincia))
            utilizador.Provincia = request.Provincia;

        if (request.EscolaId.HasValue)
        {
            // Verify escola exists
            var escolaExists = await _dbContext.Escolas
                .AnyAsync(e => e.Id == request.EscolaId, cancellationToken);

            if (!escolaExists)
                return BadRequest(new { message = "Escola não encontrada" });

            utilizador.EscolaId = request.EscolaId;
        }

        if (request.TurmaId.HasValue)
        {
            // Verify turma exists
            var turmaExists = await _dbContext.Turmas
                .AnyAsync(t => t.Id == request.TurmaId, cancellationToken);

            if (!turmaExists)
                return BadRequest(new { message = "Turma não encontrada" });

            utilizador.TurmaId = request.TurmaId;
        }

        _dbContext.Utilizadores.Update(utilizador);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var response = MapToPerfilResponseDto(utilizador);
        return Ok(response);
    }

    /// <summary>
    /// Lists all favorite contents for the authenticated user
    /// </summary>
    [HttpGet("favoritos")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PagedResult<ConteudoResponseDto>>> GetFavoritos(
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanho = 20,
        CancellationToken cancellationToken = default)
    {
        if (pagina < 1) pagina = 1;
        if (tamanho < 1 || tamanho > 100) tamanho = 20;

        var userIdClaim = User.FindFirst("sub")?.Value;
        if (!int.TryParse(userIdClaim, out var userId))
            return Unauthorized(new { message = "Utilizador não autenticado" });

        var totalCount = await _dbContext.Favoritos
            .Where(f => f.UtilizadorId == userId)
            .CountAsync(cancellationToken);

        var favoritos = await _dbContext.Favoritos
            .Where(f => f.UtilizadorId == userId)
            .Include(f => f.Conteudo)
            .ThenInclude(c => c.Editor)
            .OrderByDescending(f => f.DataAdicionado)
            .Skip((pagina - 1) * tamanho)
            .Take(tamanho)
            .Select(f => f.Conteudo)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var response = favoritos.Select(c => new ConteudoResponseDto(
            c.Id,
            c.Titulo,
            c.Resumo,
            c.CorpoTexto,
            c.VideoUrl,
            c.AudioUrl,
            c.ThumbnailUrl,
            c.Tipo,
            c.Nivel,
            c.Tema,
            c.Regiao,
            c.Estado,
            c.EditorId,
            c.Editor?.Nome,
            c.Visualizacoes,
            true,
            c.IsJindungo,
            c.ReferenciaFactual,
            c.DataPublicacao)).ToList();

        var pagedResult = PagedResult<ConteudoResponseDto>.Create(response, totalCount, pagina, tamanho);

        Response.Headers.Add("X-Total-Count", totalCount.ToString());
        Response.Headers.Add("X-Page", pagina.ToString());
        Response.Headers.Add("X-Page-Size", tamanho.ToString());

        return Ok(pagedResult);
    }

    private PerfilResponseDto MapToPerfilResponseDto(Core.Entities.Utilizador utilizador)
    {
        return new PerfilResponseDto(
            utilizador.Id,
            utilizador.Nome,
            utilizador.Email,
            utilizador.Telemovel,
            utilizador.Tipo,
            utilizador.DataRegisto,
            utilizador.PontosTotais,
            utilizador.StreakAtual,
            utilizador.Provincia,
            utilizador.EscolaId,
            utilizador.Escola?.Nome,
            utilizador.TurmaId,
            utilizador.Turma?.Nome);
    }
}
