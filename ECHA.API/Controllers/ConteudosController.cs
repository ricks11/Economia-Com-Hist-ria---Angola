using EconomiaComHistoria.API.Services;
using EconomiaComHistoria.Core.DTOs;
using EconomiaComHistoria.Core.DTOs.Sync;
using EconomiaComHistoria.Core.Entities;
using EconomiaComHistoria.Core.Enums;
using EconomiaComHistoria.Core.Helpers;
using EconomiaComHistoria.Core.Interfaces;
using EconomiaComHistoria.Infrastructure.Data;
using EconomiaComHistoria.Infrastructure.Repositories;
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
    private readonly IConteudoCacheExportService _conteudoCacheService;
    private readonly IConteudoRepository _conteudoRepository;

    public ConteudosController(AppDbContext dbContext, IFileStorageService fileStorageService, IConteudoCacheExportService conteudoCacheService, IConteudoRepository conteudoRepository )
    {
        _dbContext = dbContext;
        _fileStorageService = fileStorageService;
        _conteudoCacheService = conteudoCacheService;
        _conteudoRepository = conteudoRepository;
    }

    /// <summary>
    /// Obtém conteúdos otimizados para download offline
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
    /// Cria um novo item de conteúdo (Apenas Editor, Professor, Admin)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Editor,Professor,Admin,SuperAdmin")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ConteudoResponseDto>> CreateConteudo(
        [FromBody] CreateConteudoDto request,
        CancellationToken cancellationToken)
    {
        // 1. Regras de negócio específicas para o Jindungo
        if (request.IsJindungo && string.IsNullOrWhiteSpace(request.ReferenciaFactual))
            return BadRequest(new { message = "Referência factual é obrigatória para conteúdo Jindungo" });

        // 2. Recupera o ID do utilizador autenticado via Claim "sub" ou NameIdentifier
        var userIdClaim = User.FindFirst("sub")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out var userId))
            return Unauthorized(new { message = "Utilizador não autenticado" });

        // 3. Criação da entidade alinhada com as propriedades recebidas do CreateConteudoDto
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

            // Forçamos o Estado para Publicado (2) para garantir que aparece no Index da listagem
            Estado = EstadoConteudo.Publicado,

            DataAgendada = request.DataAgendada,
            IsJindungo = request.IsJindungo,
            ReferenciaFactual = request.IsJindungo ? request.ReferenciaFactual : null,

            // Sincronização dos campos multimédia com base no Enum TipoConteudo
            VideoUrl = request.Tipo == TipoConteudo.Video ? request.VideoUrl : null,
            AudioUrl = request.Tipo == TipoConteudo.Podcast ? request.AudioUrl : null,
            ThumbnailUrl = request.ThumbnailUrl
        };

        // 4. Salva fisicamente na base de dados
        _dbContext.Conteudos.Add(conteudo);
        await _dbContext.SaveChangesAsync(cancellationToken);

        // 5. Mapeia para o DTO de Resposta Oficial
        var response = MapToResponseDto(conteudo, false);
        return CreatedAtAction(nameof(GetConteudo), new { id = conteudo.Id }, response);
    }

    /// <summary>
    /// Lista todos os conteúdos com filtros opcionais e paginação
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    [ResponseCache(Duration = 600, VaryByQueryKeys = new[] { "tema", "nivel", "tipo", "regiao", "pagina", "tamanho", "estado", "jindungo" })]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<ConteudoResponseDto>>> ListConteudos(
        [FromQuery] string? tema,
        [FromQuery] NivelDificuldade? nivel,
        [FromQuery] TipoConteudo? tipo,
        [FromQuery] string? regiao,
        [FromQuery] EstadoConteudo? estado, 
        [FromQuery] bool? jindungo,
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanho = 20,
        CancellationToken cancellationToken = default)
    {
        if (pagina < 1) pagina = 1;
        if (tamanho < 1 || tamanho > 100) tamanho = 20;

        var userIdClaim = User.FindFirst("sub")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userId = int.TryParse(userIdClaim, out var uid) ? uid : 0;

        var query = _dbContext.Conteudos.AsNoTracking();

        // Aplica o filtro de estado se fornecido, senão assume apenas publicados por omissão
        if (estado.HasValue)
            query = query.Where(c => c.Estado == estado.Value);
        else
            query = query.Where(c => c.Estado == EstadoConteudo.Publicado);

        // Dentro do método ListConteudos da tua API C#
        if (jindungo.HasValue)
        {
            // Se for true -> traz apenas com jindungo. Se for false -> traz apenas sem jindungo.
            query = query.Where(c => c.IsJindungo == jindungo.Value);
        }

        if (!string.IsNullOrEmpty(tema)) query = query.Where(c => c.Tema == tema);
        if (nivel.HasValue) query = query.Where(c => c.Nivel == nivel.Value);

        if (!string.IsNullOrEmpty(tema)) query = query.Where(c => c.Tema == tema);
        if (nivel.HasValue) query = query.Where(c => c.Nivel == nivel.Value);
        if (tipo.HasValue) query = query.Where(c => c.Tipo == tipo.Value);
        if (!string.IsNullOrEmpty(regiao)) query = query.Where(c => c.Regiao == regiao);

        var totalCount = await query.CountAsync(cancellationToken);

        var conteudos = await query
            .Include(c => c.Editor)
            .OrderByDescending(c => c.DataPublicacao)
            .Skip((pagina - 1) * tamanho)
            .Take(tamanho)
            .ToListAsync(cancellationToken);

        // Mapeamento dinâmico verificando se cada item consta nos favoritos do utilizador logado
        var response = conteudos.Select(c => MapToResponseDto(c, userId > 0 &&
            _dbContext.Favoritos.Any(f => f.ConteudoId == c.Id && f.UtilizadorId == userId))).ToList();

        var pagedResult = PagedResult<ConteudoResponseDto>.Create(response, totalCount, pagina, tamanho);

        Response.Headers["X-Total-Count"] = totalCount.ToString();
        Response.Headers["X-Page"] = pagina.ToString();
        Response.Headers["X-Page-Size"] = tamanho.ToString();

        return Ok(pagedResult);
    }

    /// <summary>
    /// Obtém um conteúdo específico por ID
    /// </summary>
    [HttpGet("{id}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ConteudoResponseDto>> GetConteudo(int id, CancellationToken cancellationToken)
    {
        var conteudo = await _dbContext.Conteudos
            .Include(c => c.Editor)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        if (conteudo is null)
            return NotFound(new { message = "Conteúdo não encontrado" });

        var userIdClaim = User.FindFirst("sub")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userId = int.TryParse(userIdClaim, out var uid) ? uid : 0;

        var isFavorito = userId > 0 && await _dbContext.Favoritos
            .AnyAsync(f => f.ConteudoId == id && f.UtilizadorId == userId, cancellationToken);

        var response = MapToResponseDto(conteudo, isFavorito);
        return Ok(response);
    }

    /// <summary>
    /// Adiciona a tradução a um conteúdo existente
    /// </summary>
    [HttpPost("{id:int}/traducoes")]
    [Authorize(Roles = "Admin,Editor")]
    public async Task<ActionResult<TraducaoResponseDto>> AdicionarTraducao(int id, [FromBody] CreateTraducaoDto request, CancellationToken cancellationToken)
    {
        var conteudo = await _dbContext.Conteudos.FindAsync(new object[] { id }, cancellationToken);
        if (conteudo is null) return NotFound(new { message = "Conteúdo não encontrado" });

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

    /// <summary>
    /// Obtém as traduções de um conteúdo específico
    /// </summary>
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
    /// Atualiza um conteúdo existente
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

        var userIdClaim = User.FindFirst("sub")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out var userId))
            return Unauthorized(new { message = "Utilizador não autenticado" });

        var roleClaim = User.FindFirst(ClaimTypes.Role)?.Value;
        var isAdmin = roleClaim is "Admin" or "SuperAdmin";
        var isEditor = conteudo.EditorId == userId;

        if (!isEditor && !isAdmin)
            return Forbid();

        // Validação condicional para a referência do Jindungo no Update
        if (request.IsJindungo == true && string.IsNullOrWhiteSpace(request.ReferenciaFactual) && string.IsNullOrWhiteSpace(conteudo.ReferenciaFactual))
            return BadRequest(new { message = "Conteúdo Jindungo requer referência factual." });

        // Atualização incremental baseada no preenchimento do UpdateConteudoDto
        if (!string.IsNullOrWhiteSpace(request.Titulo)) conteudo.Titulo = request.Titulo;
        if (request.Resumo is not null) conteudo.Resumo = request.Resumo;
        if (request.CorpoTexto is not null) conteudo.CorpoTexto = request.CorpoTexto;
        if (request.VideoUrl is not null) conteudo.VideoUrl = request.VideoUrl;
        if (request.AudioUrl is not null) conteudo.AudioUrl = request.AudioUrl;
        if (request.ThumbnailUrl is not null) conteudo.ThumbnailUrl = request.ThumbnailUrl;
        if (request.Tema is not null) conteudo.Tema = request.Tema;
        if (request.Nivel.HasValue) conteudo.Nivel = request.Nivel.Value;
        if (request.Regiao is not null) conteudo.Regiao = request.Regiao;
        if (request.Tipo.HasValue) conteudo.Tipo = request.Tipo.Value;
        if (request.Estado.HasValue) conteudo.Estado = request.Estado.Value;
        if (request.DataAgendada.HasValue) conteudo.DataAgendada = request.DataAgendada.Value;

        if (request.IsJindungo.HasValue)
        {
            conteudo.IsJindungo = request.IsJindungo.Value;
            if (!request.IsJindungo.Value)
                conteudo.ReferenciaFactual = null;
        }

        if (request.ReferenciaFactual is not null && conteudo.IsJindungo)
            conteudo.ReferenciaFactual = request.ReferenciaFactual;

        conteudo.AtualizadoEm = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        var isFavorito = await _dbContext.Favoritos
            .AnyAsync(f => f.ConteudoId == id && f.UtilizadorId == userId, cancellationToken);

        return Ok(MapToResponseDto(conteudo, isFavorito));
    }

    [HttpGet("{id}/offline-package")]
    public async Task<ActionResult<ConteudoOfflinePacoteDto>> GetConteudoOffline(int id)
    {
        var pacote = await _conteudoCacheService.ExportarParaCacheAsync(id);
        if (pacote == null) return NotFound();
        return Ok(pacote);
    }

    /// <summary>
    /// Arquiva temporariamente um conteúdo (Soft Delete)
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> DeleteConteudo(int id, CancellationToken cancellationToken)
    {
        var conteudo = await _dbContext.Conteudos.FindAsync(new object[] { id }, cancellationToken);
        if (conteudo is null) return NotFound(new { message = "Conteúdo não encontrado" });

        var userIdClaim = User.FindFirst("sub")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out var userId)) return Unauthorized();

        var roleClaim = User.FindFirst(ClaimTypes.Role)?.Value;
        if (conteudo.EditorId != userId && roleClaim != "Admin") return Forbid();

        conteudo.Estado = EstadoConteudo.Arquivado;
        _dbContext.Conteudos.Update(conteudo);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Faz o upload ou substituição da imagem de capa (ThumbnailUrl)
    /// </summary>
    [HttpPost("{id}/imagem")]
    [Authorize]
    public async Task<ActionResult<ConteudoResponseDto>> UploadImagemCapa(int id, IFormFile imagem, CancellationToken cancellationToken)
    {
        var conteudo = await _dbContext.Conteudos.FindAsync(new object[] { id }, cancellationToken);
        if (conteudo is null) return NotFound(new { message = "Conteúdo não encontrado" });

        var userIdClaim = User.FindFirst("sub")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out var userId)) return Unauthorized();

        var roleClaim = User.FindFirst(ClaimTypes.Role)?.Value;
        if (conteudo.EditorId != userId && roleClaim != "Admin") return Forbid();

        try
        {
            if (!string.IsNullOrEmpty(conteudo.ThumbnailUrl))
                await _fileStorageService.DeleteFileAsync(conteudo.ThumbnailUrl);

            conteudo.ThumbnailUrl = await _fileStorageService.UploadFileAsync(imagem.OpenReadStream(), imagem.FileName, "uploads/conteudos");

            _dbContext.Conteudos.Update(conteudo);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return Ok(MapToResponseDto(conteudo, false));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Regista visualizações únicas por utilizador
    /// </summary>
    [HttpPost("{id}/visualizacao")]
    [Authorize]
    public async Task<IActionResult> RegistrarVisualizacao(int id, CancellationToken cancellationToken)
    {
        var conteudo = await _dbContext.Conteudos.FindAsync(new object[] { id }, cancellationToken);
        if (conteudo is null) return NotFound(new { message = "Conteúdo não encontrado" });

        var userIdClaim = User.FindFirst("sub")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out var userId)) return Unauthorized();

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
    /// Alterna o estado de favorito do conteúdo para o utilizador atual
    /// </summary>
    [HttpPost("{id}/favorito")]
    [Authorize]
    public async Task<ActionResult<object>> ToggleFavorito(int id, CancellationToken cancellationToken)
    {
        var conteudo = await _dbContext.Conteudos.FindAsync(new object[] { id }, cancellationToken);
        if (conteudo is null) return NotFound(new { message = "Conteúdo não encontrado" });

        var userIdClaim = User.FindFirst("sub")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out var userId)) return Unauthorized();

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

    // Auxiliar centralizado para mapear estritamente para o record ConteudoResponseDto fornecido
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
            conteudo.Tipo, // TipoConteudo (Video, Texto, Podcast)
            conteudo.Nivel,
            conteudo.Tema,
            conteudo.Regiao,
            conteudo.Estado,
            conteudo.EditorId,
            conteudo.Editor?.Nome, // Mapeia para EditorNome no Record DTO
            conteudo.Visualizacoes,
            ehFavorito,
            conteudo.IsJindungo,
            conteudo.ReferenciaFactual,
            conteudo.DataPublicacao);
    }

    public async Task IncrementarVisitaAsync(int id)
    {
        // 1. Busca através do repositório
        var conteudo = await _conteudoRepository.GetByIdAsync(id);
        if (conteudo == null) throw new Exception("Conteúdo não encontrado");

        // 2. Incrementa
        conteudo.Visualizacoes += 1;

        // 3. Persiste através do repositório
        await _conteudoRepository.UpdateAsync(conteudo);
    }
}