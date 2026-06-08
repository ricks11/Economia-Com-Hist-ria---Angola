using EconomiaComHistoria.Core.Entities;
using EconomiaComHistoria.Core.Enums;
using EconomiaComHistoria.API.Services;
using EconomiaComHistoria.Core.Helpers;
using EconomiaComHistoria.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using EconomiaComHistoria.Core.DTOs;
using EconomiaComHistoria.Core.Interfaces;

namespace EconomiaComHistoria.API.Controllers;

[ApiController]
[Route("api/conteudos")]
public class ConteudosController : ControllerBase
{
    private readonly AppDbContext _dbContext;
    private readonly IFileStorageService _fileStorageService;

    public ConteudosController(AppDbContext dbContext, IFileStorageService fileStorageService)
    {
        _dbContext = dbContext;
        _fileStorageService = fileStorageService;
    }

    /// <summary>
    /// Gets content optimized for offline download
    /// </summary>
    [HttpGet("download")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<object>>> DownloadConteudosCompacto(CancellationToken cancellationToken)
    {
        var conteudos = await _dbContext.Conteudos
            .Where(c => c.Estado == EstadoConteudo.Publicado)
            .Select(c => new {
                c.Id,
                c.Titulo,
                c.Resumo,
                c.CorpoTexto,
                c.ThumbnailUrl,
                c.Tipo
            })
            .ToListAsync(cancellationToken);

        return Ok(conteudos);
    }

    /// <summary>
    /// Creates a new content item (Editor/Admin only)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Editor,Professor,Admin")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ConteudoResponseDto>> CreateConteudo(
        [FromBody] CreateConteudoDto request,
        CancellationToken cancellationToken)
    {
        // Validate input
        if (string.IsNullOrWhiteSpace(request.Titulo))
            return BadRequest(new { message = "Título é obrigatório" });

        // Get current user ID from JWT
        var userIdClaim = User.FindFirst("sub")?.Value;
        if (!int.TryParse(userIdClaim, out var userId))
            return Unauthorized(new { message = "Utilizador não autenticado" });

        if (request.IsJindungo && string.IsNullOrEmpty(request.ReferenciaFactual))
            return BadRequest(new { message = "Referência factual é obrigatória para conteúdo Jindungo" });

        // Create content
        var conteudo = new Conteudo
        {
            Titulo = request.Titulo,
            Resumo = request.Resumo,
            CorpoTexto = request.CorpoTexto,
            Tema = request.Tema,
            Nivel = request.Nivel,
            Regiao = request.Regiao,
            Tipo = request.Tipo,
            EditorId = userId,
            DataPublicacao = DateTime.UtcNow,
            Estado = EstadoConteudo.Publicado
        };

        _dbContext.Conteudos.Add(conteudo);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var response = MapToResponseDto(conteudo, false);
        return CreatedAtAction(nameof(GetConteudo), new { id = conteudo.Id }, response);
    }

    /// <summary>
    /// Lists all content with optional filtering and pagination
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<ConteudoResponseDto>>> ListConteudos(
        [FromQuery] string? tema,
        [FromQuery] NivelDificuldade? nivel,
        [FromQuery] TipoConteudo? tipo,
        [FromQuery] string? regiao,
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanho = 20,
        CancellationToken cancellationToken = default)
    {
        if (pagina < 1) pagina = 1;
        if (tamanho < 1 || tamanho > 100) tamanho = 20;

        var userIdClaim = User.FindFirst("sub")?.Value;
        var userId = int.TryParse(userIdClaim, out var uid) ? uid : 0;

        var query = _dbContext.Conteudos
            .Where(c => c.Estado == EstadoConteudo.Publicado)
            .AsNoTracking();

        // Apply filters
        if (!string.IsNullOrEmpty(tema)) query = query.Where(c => c.Tema == tema);
        if (nivel.HasValue) query = query.Where(c => c.Nivel == nivel.Value);
        if (tipo.HasValue) query = query.Where(c => c.Tipo == tipo.Value);
        if (!string.IsNullOrEmpty(regiao)) query = query.Where(c => c.Regiao == regiao);
    

        var totalCount = await query.CountAsync(cancellationToken);

        var conteudos = await query
            .Include(c => c.Editor)
            .Skip((pagina - 1) * tamanho)
            .Take(tamanho)
            .OrderByDescending(c => c.DataPublicacao)
            .ToListAsync(cancellationToken);

        var response = conteudos.Select(c => MapToResponseDto(c, userId > 0 && 
            _dbContext.Favoritos.Any(f => f.ConteudoId == c.Id && f.UtilizadorId == userId))).ToList();

        var pagedResult = PagedResult<ConteudoResponseDto>.Create(response, totalCount, pagina, tamanho);

        Response.Headers["X-Total-Count"] = totalCount.ToString();
        Response.Headers["X-Page"] = pagina.ToString();
        Response.Headers["X-Page-Size"] = tamanho.ToString();

        return Ok(pagedResult);
    }

    /// <summary>
    /// Gets a specific content item (public access)
    /// </summary>
    [HttpGet("{id}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ConteudoResponseDto>> GetConteudo(
        int id,
        CancellationToken cancellationToken)
    {
        var conteudo = await _dbContext.Conteudos
            .Include(c => c.Editor)
            .FirstOrDefaultAsync(c => c.Id == id && c.Estado == EstadoConteudo.Publicado, cancellationToken);

        if (conteudo is null)
            return NotFound(new { message = "Conteúdo não encontrado" });

        var userIdClaim = User.FindFirst("sub")?.Value;
        var userId = int.TryParse(userIdClaim, out var uid) ? uid : 0;

        var isFavorito = userId > 0 && await _dbContext.Favoritos
            .AnyAsync(f => f.ConteudoId == id && f.UtilizadorId == userId, cancellationToken);

        var response = MapToResponseDto(conteudo, isFavorito);
        return Ok(response);
    }

    [HttpPost("{id:int}/traducoes")]
    [Authorize(Roles = "Admin,Editor")]
    public async Task<ActionResult<TraducaoResponseDto>> AdicionarTraducao(int id, [FromBody] CreateTraducaoDto request, CancellationToken cancellationToken)
    {
        var conteudo = await _dbContext.Conteudos.FindAsync(new object[] { id }, cancellationToken);
        if (conteudo is null) return NotFound();

        var traducao = new ConteudoTraducao
        {
            ConteudoId = id,
            Lingua = request.Lingua,
            TextoTraduzido = request.Texto,
            AudioUrl = request.AudioUrl
        };

        _dbContext.TraducoesConteudo.Add(traducao);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Ok(new TraducaoResponseDto(traducao.Id, traducao.Lingua, traducao.TextoTraduzido, traducao.AudioUrl));
    }

    [HttpGet("{id:int}/traducoes")]
    public async Task<ActionResult<IEnumerable<TraducaoResponseDto>>> GetTraducoes(int id, CancellationToken cancellationToken)
    {
        var traducoes = await _dbContext.TraducoesConteudo
            .Where(t => t.ConteudoId == id)
            .Select(t => new TraducaoResponseDto(t.Id, t.Lingua, t.TextoTraduzido, t.AudioUrl))
            .ToListAsync(cancellationToken);

        return Ok(traducoes);
    }

    /// <summary>
    /// Updates an existing content item
    /// </summary>
    [HttpPut("{id}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ConteudoResponseDto>> UpdateConteudo(
    int id,
    [FromBody] UpdateConteudoDto request,
    CancellationToken cancellationToken)
    {
        var conteudo = await _dbContext.Conteudos
            .Include(c => c.Editor)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        if (conteudo is null)
            return NotFound(new { message = "Conteúdo não encontrado" });

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                       ?? User.FindFirst("sub")?.Value;
        if (!int.TryParse(userIdClaim, out var userId))
            return Unauthorized(new { message = "Utilizador não autenticado" });

        var roleClaim = User.FindFirst(ClaimTypes.Role)?.Value;
        var isAdmin = roleClaim is "Admin" or "SuperAdmin";
        var isEditor = conteudo.EditorId == userId;

        if (!isEditor && !isAdmin)
            return Forbid();

        // Valida Jindungo — se activado, referência factual é obrigatória
        if (request.IsJindungo == true && string.IsNullOrWhiteSpace(request.ReferenciaFactual))
            return BadRequest(new { message = "Conteúdo Jindungo requer referência factual." });

        if (!string.IsNullOrWhiteSpace(request.Titulo))
            conteudo.Titulo = request.Titulo;
        if (request.Resumo is not null)
            conteudo.Resumo = request.Resumo;
        if (request.CorpoTexto is not null)
            conteudo.CorpoTexto = request.CorpoTexto;
        if (request.VideoUrl is not null)
            conteudo.VideoUrl = request.VideoUrl;
        if (request.AudioUrl is not null)
            conteudo.AudioUrl = request.AudioUrl;
        if (request.ThumbnailUrl is not null)
            conteudo.ThumbnailUrl = request.ThumbnailUrl;
        if (request.Tema is not null)
            conteudo.Tema = request.Tema;
        if (request.Nivel.HasValue)
            conteudo.Nivel = request.Nivel.Value;
        if (request.Regiao is not null)
            conteudo.Regiao = request.Regiao;
        if (request.Tipo.HasValue)
            conteudo.Tipo = request.Tipo.Value;
        if (request.IsJindungo.HasValue)
        {
            conteudo.IsJindungo = request.IsJindungo.Value;
            // Se desactivar Jindungo, limpa a referência
            if (!request.IsJindungo.Value)
                conteudo.ReferenciaFactual = null;
        }
        if (request.ReferenciaFactual is not null)
            conteudo.ReferenciaFactual = request.ReferenciaFactual;

        conteudo.AtualizadoEm = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        var isFavorito = await _dbContext.Favoritos
            .AnyAsync(f => f.ConteudoId == id && f.UtilizadorId == userId, cancellationToken);

        return Ok(MapToResponseDto(conteudo, isFavorito));
    }

    /// <summary>
    /// Soft delete a content item (Author or Admin only)
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteConteudo(
        int id,
        CancellationToken cancellationToken)
    {
        var conteudo = await _dbContext.Conteudos.FindAsync(new object[] { id }, cancellationToken: cancellationToken);

        if (conteudo is null)
            return NotFound(new { message = "Conteúdo não encontrado" });

        // Get current user and check authorization
        var userIdClaim = User.FindFirst("sub")?.Value;
        if (!int.TryParse(userIdClaim, out var userId))
            return Unauthorized(new { message = "Utilizador não autenticado" });

        var roleClaim = User.FindFirst(ClaimTypes.Role)?.Value;
        var isAdmin = roleClaim == "Admin";
        var isAuthor = conteudo.EditorId == userId;

        if (!isAuthor && !isAdmin)
            return Forbid();

        // Soft delete
        conteudo.Estado = EstadoConteudo.Arquivado;
        _dbContext.Conteudos.Update(conteudo);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Uploads a cover image for content
    /// </summary>
    [HttpPost("{id}/imagem")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ConteudoResponseDto>> UploadImagemCapa(
        int id,
        IFormFile imagem,
        CancellationToken cancellationToken)
    {
        var conteudo = await _dbContext.Conteudos.FindAsync(new object[] { id }, cancellationToken: cancellationToken);

        if (conteudo is null)
            return NotFound(new { message = "Conteúdo não encontrado" });

        // Get current user and check authorization
        var userIdClaim = User.FindFirst("sub")?.Value;
        if (!int.TryParse(userIdClaim, out var userId))
            return Unauthorized(new { message = "Utilizador não autenticado" });

        var roleClaim = User.FindFirst(ClaimTypes.Role)?.Value;
        var isAdmin = roleClaim == "Admin";
        var isAuthor = conteudo.EditorId == userId;

        if (!isAuthor && !isAdmin)
            return Forbid();

        try
        {
            // Delete old image if exists
            if (!string.IsNullOrEmpty(conteudo.ThumbnailUrl))
                await _fileStorageService.DeleteFileAsync(conteudo.ThumbnailUrl);

            // Upload new image
            conteudo.ThumbnailUrl = await _fileStorageService
                .UploadFileAsync(imagem.OpenReadStream(), imagem.FileName, "uploads/conteudos");

            _dbContext.Conteudos.Update(conteudo);
            await _dbContext.SaveChangesAsync(cancellationToken);

            var response = MapToResponseDto(conteudo, false);
            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Registers a content view (authenticated users only)
    /// </summary>
    [HttpPost("{id}/visualizacao")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RegistrarVisualizacao(
        int id,
        CancellationToken cancellationToken)
    {
        var conteudo = await _dbContext.Conteudos.FindAsync(new object[] { id }, cancellationToken: cancellationToken);

        if (conteudo is null)
            return NotFound(new { message = "Conteúdo não encontrado" });

        var userIdClaim = User.FindFirst("sub")?.Value;
        if (!int.TryParse(userIdClaim, out var userId))
            return Unauthorized(new { message = "Utilizador não autenticado" });

        // Check if already viewed
        var existingView = await _dbContext.VisualizacoesConteudo
            .FirstOrDefaultAsync(v => v.ConteudoId == id && v.UtilizadorId == userId, cancellationToken);

        if (existingView is null)
        {
            var visualizacao = new VisualizacaoConteudo
            {
                ConteudoId = id,
                UtilizadorId = userId,
                DataHora = DateTime.UtcNow
            };

            _dbContext.VisualizacoesConteudo.Add(visualizacao);
            conteudo.Visualizacoes++;
            _dbContext.Conteudos.Update(conteudo);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Toggles a content as favorite (authenticated users only)
    /// </summary>
    [HttpPost("{id}/favorito")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<object>> ToggleFavorito(
        int id,
        CancellationToken cancellationToken)
    {
        var conteudo = await _dbContext.Conteudos.FindAsync(new object[] { id }, cancellationToken: cancellationToken);

        if (conteudo is null)
            return NotFound(new { message = "Conteúdo não encontrado" });

        var userIdClaim = User.FindFirst("sub")?.Value;
        if (!int.TryParse(userIdClaim, out var userId))
            return Unauthorized(new { message = "Utilizador não autenticado" });

        var favorito = await _dbContext.Favoritos
            .FirstOrDefaultAsync(f => f.ConteudoId == id && f.UtilizadorId == userId, cancellationToken);

        if (favorito is not null)
        {
            _dbContext.Favoritos.Remove(favorito);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Ok(new { adicionado = false, message = "Removido de favoritos" });
        }
        else
        {
            var novoFavorito = new ConteudoFavorito
            {
                ConteudoId = id,
                UtilizadorId = userId,
                DataAdicionado = DateTime.UtcNow
            };

            _dbContext.Favoritos.Add(novoFavorito);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Ok(new { adicionado = true, message = "Adicionado a favoritos" });
        }
    }

    private static ConteudoResponseDto MapToResponseDto(Conteudo conteudo, bool ehFavorito)
    {
        return new ConteudoResponseDto(
            conteudo.Id,
            conteudo.Titulo,
            conteudo.Resumo,
            conteudo.CorpoTexto,
            conteudo.VideoUrl,
            conteudo.AudioUrl,
            conteudo.ThumbnailUrl,
            conteudo.Tipo,
            conteudo.Nivel,
            conteudo.Tema,
            conteudo.Regiao,
            conteudo.Estado,
            conteudo.EditorId,
            conteudo.Editor?.Nome,
            conteudo.Visualizacoes,
            ehFavorito,
            conteudo.IsJindungo,
            conteudo.ReferenciaFactual,
            conteudo.DataPublicacao);
    }
}
