using EconomiaComHistoria.Core.DTOs;
using EconomiaComHistoria.Core.Entities;
using EconomiaComHistoria.Core.Enums;
using EconomiaComHistoria.Core.Helpers;
using EconomiaComHistoria.Core.Interfaces;
using EconomiaComHistoria.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ECHA.API.Controllers;

[ApiController]
[Route("api/perfil")]
[Authorize]
public class PerfilController : ControllerBase
{
    private readonly AppDbContext _dbContext;
    private readonly IAuthService _authService;

    public PerfilController(AppDbContext dbContext, IAuthService authService)
    {
        _dbContext = dbContext;
        _authService = authService;
    }

    private bool TryGetUserId(out int userId)
    {
        var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? User.FindFirst("sub")?.Value;
        return int.TryParse(userIdStr, out userId);
    }

    private async Task<Utilizador?> GetCurrentUserWithIncludesAsync(CancellationToken ct)
    {
        if (!TryGetUserId(out var userId))
            return null;

        return await _dbContext.Utilizadores
            .Include(u => u.Escola)
            .Include(u => u.Turma)
            .FirstOrDefaultAsync(u => u.Id == userId, ct);
    }

    [HttpGet("progresso")]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public async Task<ActionResult<ProgressoUtilizadorDto>> GetProgresso(CancellationToken ct)
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized(new { message = "Utilizador não autenticado" });

        var user = await _dbContext.Utilizadores
            .Include(u => u.BadgesConquistados)
            .ThenInclude(bc => bc.Badge)
            .FirstOrDefaultAsync(u => u.Id == userId, ct);

        if (user == null) return NotFound(new { message = "Utilizador não encontrado" });

        int nivel = (user.PontosTotais / 1000) + 1;
        int pontosNoNivelAtual = user.PontosTotais % 1000;
        double percentagemNivel = (double)pontosNoNivelAtual / 1000 * 100;

        var badges = user.BadgesConquistados.Select(bc => new BadgeConquistadoDto(
            bc.BadgeId,
            bc.Badge?.Nome ?? "Badge",
            bc.Badge?.Descricao,
            bc.Badge?.IconeUrl,
            bc.DataConquista
        )).ToList();

        return Ok(new ProgressoUtilizadorDto(
            user.PontosTotais,
            nivel,
            1000 - pontosNoNivelAtual,
            percentagemNivel,
            user.StreakAtual,
            badges
        ));
    }

    [HttpGet]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public async Task<ActionResult<PerfilResponseDto>> GetPerfil(CancellationToken ct)
    {
        var utilizador = await GetCurrentUserWithIncludesAsync(ct);
        if (utilizador == null)
            return Unauthorized();

        return Ok(MapToPerfilResponseDto(utilizador));
    }

    [HttpPut]
    public async Task<ActionResult<PerfilResponseDto>> UpdatePerfil(
        [FromBody] UpdatePerfilDto request,
        CancellationToken ct)
    {
        var utilizador = await GetCurrentUserWithIncludesAsync(ct);
        if (utilizador == null)
            return Unauthorized();

        // Atualizar campos simples
        if (!string.IsNullOrEmpty(request.Nome))
            utilizador.Nome = request.Nome;

        if (!string.IsNullOrEmpty(request.Provincia))
            utilizador.Provincia = request.Provincia;

        if (request.Telemovel is not null)
            utilizador.Telemovel = request.Telemovel;

        // ---- VALIDAÇÃO DE PERMISSÃO PARA ESCOLA ----
        if (request.EscolaId.HasValue)
        {
            var isAdmin = utilizador.Tipo == TipoUtilizador.Admin
                       || utilizador.Tipo == TipoUtilizador.SuperAdmin;

            if (isAdmin)
            {
                // Admin pode definir qualquer escola (desde que exista)
                var escolaExists = await _dbContext.Escolas
                    .AnyAsync(e => e.Id == request.EscolaId.Value, ct);
                if (!escolaExists)
                    return BadRequest(new { message = "Escola não encontrada" });

                utilizador.EscolaId = request.EscolaId;
            }
            else
            {
                // Utilizador comum: só pode manter a mesma escola ou remover (null)
                if (utilizador.EscolaId.HasValue && utilizador.EscolaId != request.EscolaId)
                {
                    return BadRequest(new { message = "Não tem permissão para alterar a escola. Utilize o código de convite." });
                }
                // Se for null, permite remover
                if (!request.EscolaId.HasValue)
                    utilizador.EscolaId = null;
                // Se for igual, não faz nada
            }
        }
        else
        {
            // request.EscolaId é null → remover escola (permitido para todos)
            utilizador.EscolaId = null;
        }

        // ---- VALIDAÇÃO DE PERMISSÃO PARA TURMA ----
        if (request.TurmaId.HasValue)
        {
            var turma = await _dbContext.Turmas
                .Include(t => t.Escola)
                .FirstOrDefaultAsync(t => t.Id == request.TurmaId.Value, ct);

            if (turma == null)
                return BadRequest(new { message = "Turma não encontrada" });

            var isAdmin = utilizador.Tipo == TipoUtilizador.Admin
                       || utilizador.Tipo == TipoUtilizador.SuperAdmin;

            if (!isAdmin)
            {
                // Utilizador comum: só pode definir turma se pertencer à sua escola
                if (utilizador.EscolaId == null)
                    return BadRequest(new { message = "Não tem escola associada para definir turma." });

                if (turma.EscolaId != utilizador.EscolaId)
                    return BadRequest(new { message = "Esta turma não pertence à sua escola." });
            }

            utilizador.TurmaId = request.TurmaId;
        }
        else
        {
            // Se o request enviar null, permite remover a turma
            utilizador.TurmaId = null;
        }

        await _dbContext.SaveChangesAsync(ct);

        var response = MapToPerfilResponseDto(utilizador);
        return Ok(response);
    }

    [HttpPut("avatar")]
    public async Task<ActionResult<PerfilResponseDto>> UpdateAvatar(
        [FromBody] UpdateAvatarDto request,
        CancellationToken ct)
    {
        var utilizador = await GetCurrentUserWithIncludesAsync(ct);
        if (utilizador is null) return Unauthorized();

        if (string.IsNullOrWhiteSpace(request.AvatarBase64) || request.AvatarBase64.Length > 4_000_000)
            return BadRequest(new { message = "Imagem de perfil inválida ou demasiado grande." });

        if (!request.AvatarBase64.StartsWith("data:image/", StringComparison.OrdinalIgnoreCase))
            return BadRequest(new { message = "A fotografia deve ser uma imagem." });

        utilizador.AvatarConfig = request.AvatarBase64;
        await _dbContext.SaveChangesAsync(ct);
        return Ok(MapToPerfilResponseDto(utilizador));
    }

    [HttpPut("password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto request, CancellationToken ct)
    {
        var utilizador = await GetCurrentUserWithIncludesAsync(ct);
        if (utilizador is null) return Unauthorized();

        if (string.IsNullOrWhiteSpace(request.PalavraPasseAtual) ||
            string.IsNullOrWhiteSpace(request.NovaPalavraPasse) || request.NovaPalavraPasse.Length < 8)
            return BadRequest(new { message = "A nova palavra-passe deve ter pelo menos 8 caracteres." });

        if (!_authService.VerifyPassword(request.PalavraPasseAtual, utilizador.PasswordHash))
            return BadRequest(new { message = "A palavra-passe atual está incorreta." });

        utilizador.PasswordHash = _authService.HashPassword(request.NovaPalavraPasse);
        await _dbContext.SaveChangesAsync(ct);
        return Ok(new { message = "Palavra-passe alterada com sucesso." });
    }

    [HttpGet("favoritos")]
    public async Task<ActionResult<PagedResult<ConteudoResponseDto>>> GetFavoritos(
    [FromQuery] int pagina = 1,
    [FromQuery] int tamanho = 20,
    CancellationToken ct = default)
    {
        if (pagina < 1) pagina = 1;
        if (tamanho < 1 || tamanho > 100) tamanho = 20;

        if (!TryGetUserId(out var userId))
            return Unauthorized();

        // Junção explícita com a tabela Conteudos, filtrando apenas os não arquivados
        var query = from f in _dbContext.Favoritos
                    join c in _dbContext.Conteudos on f.ConteudoId equals c.Id
                    where f.UtilizadorId == userId
                       && c.Estado != EstadoConteudo.Arquivado   // ← filtro principal
                    select c;

        var totalCount = await query.CountAsync(ct);

        var favoritos = await query
            .Include(c => c.Editor)
            .OrderByDescending(c => c.DataPublicacao) // ou use f.DataAdicionado se preferir a ordem de adição
            .Skip((pagina - 1) * tamanho)
            .Take(tamanho)
            .AsNoTracking()
            .ToListAsync(ct);

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
            true, // é favorito (vem da tabela de favoritos)
            c.IsJindungo,
            c.ReferenciaFactual,
            c.DataPublicacao)).ToList();

        var pagedResult = PagedResult<ConteudoResponseDto>.Create(response, totalCount, pagina, tamanho);

        Response.Headers["X-Total-Count"] = totalCount.ToString();
        Response.Headers["X-Page"] = pagina.ToString();
        Response.Headers["X-Page-Size"] = tamanho.ToString();

        return Ok(pagedResult);
    }

    private PerfilResponseDto MapToPerfilResponseDto(Utilizador utilizador)
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
            utilizador.Turma?.Nome,
            utilizador.AvatarConfig);
    }
}
