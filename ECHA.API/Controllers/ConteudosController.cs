using EconomiaComHistoria.Core.Entities;
using EconomiaComHistoria.Core.Enums;
using EconomiaComHistoria.API.DTOs;
using EconomiaComHistoria.API.Services;
using EconomiaComHistoria.API.Helpers;
using EconomiaComHistoria.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

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
            .Where(c => c.Estado == EstadoConteudo.Ativo)
            .Select(c => new {
                c.Id,
                c.Titulo,
                c.Resumo,
                c.Texto,
                c.UrlMedia,
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
        Conteudo conteudo = request.IsJindungo 
            ? new ConteudoJindungo 
            { 
                Titulo = request.Titulo,
                Resumo = request.Resumo,
                Texto = request.Texto,
                Tema = request.Tema,
                Nivel = request.Nivel,
                Regiao = request.Regiao,
                Tipo = request.Tipo,
                UrlMedia = request.UrlMedia,
                IsJindungo = true,
                ReferenciaFactual = request.ReferenciaFactual ?? string.Empty,
                AutorId = userId,
                DataPublicacao = DateTime.UtcNow,
                Estado = EstadoConteudo.Ativo
            }
            : new Conteudo
            {
                Titulo = request.Titulo,
                Resumo = request.Resumo,
                Texto = request.Texto,
                Tema = request.Tema,
                Nivel = request.Nivel,
                Regiao = request.Regiao,
                Tipo = request.Tipo,
                UrlMedia = request.UrlMedia,
                IsJindungo = false,
                AutorId = userId,
                DataPublicacao = DateTime.UtcNow,
                Estado = EstadoConteudo.Ativo
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
        [FromQuery] string? nivel,
        [FromQuery] string? regiao,
        [FromQuery] string? tipo,
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanho = 20,
        CancellationToken cancellationToken = default)
    {
        if (pagina < 1) pagina = 1;
        if (tamanho < 1 || tamanho > 100) tamanho = 20;

        var userIdClaim = User.FindFirst("sub")?.Value;
        var userId = int.TryParse(userIdClaim, out var uid) ? uid : 0;

        var query = _dbContext.Conteudos
            .Where(c => c.Estado == EstadoConteudo.Ativo)
            .AsNoTracking();

        // Apply filters
        if (!string.IsNullOrEmpty(tema))
            query = query.Where(c => c.Tema == tema);
        if (!string.IsNullOrEmpty(nivel))
            query = query.Where(c => c.Nivel == nivel);
        if (!string.IsNullOrEmpty(regiao))
            query = query.Where(c => c.Regiao == regiao);
        if (!string.IsNullOrEmpty(tipo))
            query = query.Where(c => c.Tipo == tipo);

        var totalCount = await query.CountAsync(cancellationToken);

        var conteudos = await query
            .Include(c => c.Autor)
            .Skip((pagina - 1) * tamanho)
            .Take(tamanho)
            .OrderByDescending(c => c.DataPublicacao)
            .ToListAsync(cancellationToken);

        var response = conteudos.Select(c => MapToResponseDto(c, userId > 0 && 
            _dbContext.ConteudosFavoritos.Any(f => f.ConteudoId == c.Id && f.UtilizadorId == userId))).ToList();

        var pagedResult = PagedResult<ConteudoResponseDto>.Create(response, totalCount, pagina, tamanho);

        Response.Headers.Add("X-Total-Count", totalCount.ToString());
        Response.Headers.Add("X-Page", pagina.ToString());
        Response.Headers.Add("X-Page-Size", tamanho.ToString());

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
            .Include(c => c.Autor)
            .FirstOrDefaultAsync(c => c.Id == id && c.Estado == EstadoConteudo.Ativo, cancellationToken);

        if (conteudo is null)
            return NotFound(new { message = "Conteúdo não encontrado" });

        var userIdClaim = User.FindFirst("sub")?.Value;
        var userId = int.TryParse(userIdClaim, out var uid) ? uid : 0;

        var isFavorito = userId > 0 && await _dbContext.ConteudosFavoritos
            .AnyAsync(f => f.ConteudoId == id && f.UtilizadorId == userId, cancellationToken);

        var response = MapToResponseDto(conteudo, false);
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
            Texto = request.Texto,
            AudioUrl = request.AudioUrl
        };

        _dbContext.ConteudoTraducoes.Add(traducao);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Ok(new TraducaoResponseDto(traducao.Id, traducao.Lingua, traducao.Texto, traducao.AudioUrl));
        }

        [HttpGet("{id:int}/traducoes")]
        public async Task<ActionResult<IEnumerable<TraducaoResponseDto>>> GetTraducoes(int id, CancellationToken cancellationToken)
        {
        var traducoes = await _dbContext.ConteudoTraducoes
            .Where(t => t.ConteudoId == id)
            .Select(t => new TraducaoResponseDto(t.Id, t.Lingua, t.Texto, t.AudioUrl))
            .ToListAsync(cancellationToken);

        return Ok(traducoes);
        }

        /// <summary>
        /// Soft delete a content item (Author or Admin only)
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
        var conteudo = await _dbContext.Conteudos.FindAsync(new object[] { id }, cancellationToken: cancellationToken);

        if (conteudo is null)
            return NotFound(new { message = "Conteúdo não encontrado" });

        // Get current user and check authorization
        var userIdClaim = User.FindFirst("sub")?.Value;
        if (!int.TryParse(userIdClaim, out var userId))
            return Unauthorized(new { message = "Utilizador não autenticado" });

        var roleClaim = User.FindFirst(ClaimTypes.Role)?.Value;
        var isAdmin = roleClaim == "Admin";
        var isAuthor = conteudo.AutorId == userId;

        if (!isAuthor && !isAdmin)
            return Forbid();

        // Update properties
        if (!string.IsNullOrEmpty(request.Titulo))
            conteudo.Titulo = request.Titulo;
        if (request.Resumo != null)
            conteudo.Resumo = request.Resumo;
        if (request.Texto != null)
            conteudo.Texto = request.Texto;
        if (request.Tema != null)
            conteudo.Tema = request.Tema;
        if (request.Nivel != null)
            conteudo.Nivel = request.Nivel;
        if (request.Regiao != null)
            conteudo.Regiao = request.Regiao;
        if (request.Tipo != null)
            conteudo.Tipo = request.Tipo;
        if (request.UrlMedia != null)
            conteudo.UrlMedia = request.UrlMedia;
        if (request.IsJindungo.HasValue)
            conteudo.IsJindungo = request.IsJindungo.Value;
        if (request.ReferenciaFactual != null && conteudo is ConteudoJindungo cj)
            cj.ReferenciaFactual = request.ReferenciaFactual;

        _dbContext.Conteudos.Update(conteudo);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var response = MapToResponseDto(conteudo, false);
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
            Texto = request.Texto,
            AudioUrl = request.AudioUrl
        };

        _dbContext.ConteudoTraducoes.Add(traducao);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Ok(new TraducaoResponseDto(traducao.Id, traducao.Lingua, traducao.Texto, traducao.AudioUrl));
    }

    [HttpGet("{id:int}/traducoes")]
    public async Task<ActionResult<IEnumerable<TraducaoResponseDto>>> GetTraducoes(int id, CancellationToken cancellationToken)
    {
        var traducoes = await _dbContext.ConteudoTraducoes
            .Where(t => t.ConteudoId == id)
            .Select(t => new TraducaoResponseDto(t.Id, t.Lingua, t.Texto, t.AudioUrl))
            .ToListAsync(cancellationToken);

        return Ok(traducoes);
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
        var isAuthor = conteudo.AutorId == userId;

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
        var isAuthor = conteudo.AutorId == userId;

        if (!isAuthor && !isAdmin)
            return Forbid();

        try
        {
            // Delete old image if exists
            if (!string.IsNullOrEmpty(conteudo.ImagemCapa))
                await _fileStorageService.DeleteFileAsync(conteudo.ImagemCapa);

            // Upload new image
            conteudo.ImagemCapa = await _fileStorageService.UploadFileAsync(imagem, "uploads/conteudos");

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
                DataVisualizacao = DateTime.UtcNow
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

        var favorito = await _dbContext.ConteudosFavoritos
            .FirstOrDefaultAsync(f => f.ConteudoId == id && f.UtilizadorId == userId, cancellationToken);

        if (favorito is not null)
        {
            _dbContext.ConteudosFavoritos.Remove(favorito);
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

            _dbContext.ConteudosFavoritos.Add(novoFavorito);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Ok(new { adicionado = true, message = "Adicionado a favoritos" });
        }
    }

    private ConteudoResponseDto MapToResponseDto(Conteudo conteudo, bool ehFavorito)
    {
        return new ConteudoResponseDto(
            conteudo.Id,
            conteudo.Titulo,
            conteudo.Resumo,
            conteudo.Texto,
            conteudo.DataPublicacao,
            conteudo.AutorId,
            conteudo.Autor?.Nome,
            conteudo.Tema,
            conteudo.Nivel,
            conteudo.Regiao,
            conteudo.Tipo,
            conteudo.Estado,
            conteudo.ImagemCapa,
            conteudo.UrlMedia,
            conteudo.Visualizacoes,
            ehFavorito);
    }
}
