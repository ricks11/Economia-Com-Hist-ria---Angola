using System.Security.Claims;
using EconomiaComHistoria.Core.DTOs;
using EconomiaComHistoria.Core.Entities;
using EconomiaComHistoria.Core.Enums;
using EconomiaComHistoria.Core.Interfaces;
using EconomiaComHistoria.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EconomiaComHistoria.API.Controllers;

[ApiController]
[Route("api/forum")]
public class ForumController : ControllerBase
{
    private readonly AppDbContext _dbContext;
    private readonly IModeracaoService _moderacaoService;
    private readonly INotificacaoService _notificacaoService;

    public ForumController(
        AppDbContext dbContext,
        IModeracaoService moderacaoService,
        INotificacaoService notificacaoService)
    {
        _dbContext = dbContext;
        _moderacaoService = moderacaoService;
        _notificacaoService = notificacaoService;
    }

    // ─────────────────────────────────────────
    // TÓPICOS
    // ─────────────────────────────────────────

    [HttpPost("topicos")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<TopicoForumDto>> CriarTopico(
        [FromBody] CriarTopicoForumDto request,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized(new { message = "Utilizador não autenticado" });

        if (string.IsNullOrWhiteSpace(request.Titulo) || string.IsNullOrWhiteSpace(request.Descricao))
            return BadRequest(new { message = "Título e descrição são obrigatórios" });

        var utilizador = await _dbContext.Utilizadores
            .FindAsync(new object[] { userId }, cancellationToken);
        if (utilizador is null)
            return Unauthorized(new { message = "Utilizador não encontrado" });

        var categoria = await _dbContext.CategoriasForum
            .FindAsync(new object[] { request.CategoriaId }, cancellationToken);
        if (categoria is null)
            return BadRequest(new { message = "Categoria não encontrada" });

        var requereAprovacao = await _moderacaoService
            .RequereAprovacaoAsync(utilizador, cancellationToken);

        var topico = new TopicoForum
        {
            Titulo = request.Titulo.Trim(),
            Descricao = request.Descricao.Trim(),
            CategoriaId = request.CategoriaId,
            AutorId = userId,
            Estado = requereAprovacao
                ? EstadoTopicoForum.Pendente
                : EstadoTopicoForum.Ativo,
            CriadoEm = DateTime.UtcNow
        };

        _dbContext.TopicosForum.Add(topico);
        await _dbContext.SaveChangesAsync(cancellationToken);

        // Recarrega com Categoria para o mapeamento
        await _dbContext.Entry(topico)
            .Reference(t => t.Categoria)
            .LoadAsync(cancellationToken);

        return CreatedAtAction(
            nameof(GetTopico),
            new { id = topico.Id },
            MapTopico(topico));
    }

    [HttpGet("categorias")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<object>>> ListarCategorias(
        CancellationToken cancellationToken)
    {
        var categorias = await _dbContext.CategoriasForum
            .AsNoTracking()
            .OrderBy(c => c.Nome)
            .ToListAsync(cancellationToken);

        if (categorias.Count == 0)
        {
            var defaults = new[]
            {
                new CategoriaForum { Nome = "Economia", Descricao = "Debates sobre economia angolana", Icone = "payments" },
                new CategoriaForum { Nome = "História", Descricao = "Narrativas e factos históricos", Icone = "history_edu" },
                new CategoriaForum { Nome = "Política", Descricao = "Políticas públicas e governação", Icone = "gavel" },
                new CategoriaForum { Nome = "Sociedade", Descricao = "Cultura e transformação social", Icone = "groups" }
            };
            _dbContext.CategoriasForum.AddRange(defaults);
            await _dbContext.SaveChangesAsync(cancellationToken);
            categorias = defaults.ToList();
        }

        return Ok(categorias.Select(c => new { c.Id, c.Nome, c.Descricao, c.Icone }));
    }

    [HttpGet("topicos")]
    [AllowAnonymous]
    [ResponseCache(Duration = 120, VaryByQueryKeys = new[] { "categoriaId", "ordem" })]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<TopicoForumDto>>> ListarTopicos(
        [FromQuery] int? categoriaId,
        [FromQuery] string? ordem,
        CancellationToken cancellationToken)
    {
        var query = _dbContext.TopicosForum
            .Include(x => x.Autor)
            .Include(x => x.Categoria)
            .Where(x => x.Estado == EstadoTopicoForum.Ativo)
            .AsNoTracking();

        if (categoriaId.HasValue)
            query = query.Where(x => x.CategoriaId == categoriaId.Value);

        query = ordem?.Equals("activo", StringComparison.OrdinalIgnoreCase) == true
            ? query.OrderByDescending(x => x.Visualizacoes)
            : query.OrderByDescending(x => x.CriadoEm);

        var topicos = await query.ToListAsync(cancellationToken);
        return Ok(topicos.Select(MapTopico));
    }

    [HttpGet("topicos/{id:int}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TopicoForumDetalheDto>> GetTopico(
        int id,
        CancellationToken cancellationToken)
    {
        var topico = await _dbContext.TopicosForum
            .Include(x => x.Autor)
            .Include(x => x.Categoria)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id
                && x.Estado == EstadoTopicoForum.Ativo, cancellationToken);

        if (topico is null)
            return NotFound(new { message = "Tópico não encontrado" });

        // Incrementa visualizações
        var topicoTracked = await _dbContext.TopicosForum.FindAsync(
            new object[] { id }, cancellationToken);
        if (topicoTracked is not null)
        {
            topicoTracked.Visualizacoes++;
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        var respostas = await _dbContext.RespostasForum
            .Include(x => x.Autor)
            .Include(x => x.RespostasFilhas)
                .ThenInclude(x => x.Autor)
            .Where(x => x.TopicoId == id
                && x.EstadoResposta == EstadoComentario.Publicado
                && x.RespostaPaiId == null)
            .OrderBy(x => x.DataCriacao)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return Ok(new TopicoForumDetalheDto(
            topico.Id,
            topico.Titulo,
            topico.Descricao,
            topico.CategoriaId,
            topico.Categoria!.Nome,
            topico.AutorId,
            topico.Autor?.Nome,
            topico.Estado,
            topico.CriadoEm,
            topico.Fixado,
            topico.Visualizacoes,
            BuildRespostaTree(respostas, null, 2)));
    }

    [HttpDelete("topicos/{id:int}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ApagarTopico(
        int id,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized(new { message = "Utilizador não autenticado" });

        var topico = await _dbContext.TopicosForum
            .FindAsync(new object[] { id }, cancellationToken);
        if (topico is null)
            return NotFound(new { message = "Tópico não encontrado" });

        if (topico.AutorId != userId && !IsModerator())
            return Forbid();

        topico.Estado = EstadoTopicoForum.Arquivado;
        await _dbContext.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    // ─────────────────────────────────────────
    // RESPOSTAS
    // ─────────────────────────────────────────

    [HttpPost("topicos/{topicoId:int}/respostas")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RespostaForumDto>> AdicionarResposta(
        int topicoId,
        [FromBody] CriarRespostaForumDto request,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized(new { message = "Utilizador não autenticado" });

        if (string.IsNullOrWhiteSpace(request.Conteudo))
            return BadRequest(new { message = "Conteúdo é obrigatório" });

        var topico = await _dbContext.TopicosForum
            .FindAsync(new object[] { topicoId }, cancellationToken);
        if (topico is null || topico.Estado != EstadoTopicoForum.Ativo)
            return NotFound(new { message = "Tópico não encontrado ou inactivo" });

        if (topico.ComentariosDesativados)
            return BadRequest(new { message = "Respostas desactivadas neste tópico" });

        if (request.RespostaPaiId.HasValue)
        {
            var paiExiste = await _dbContext.RespostasForum
                .AnyAsync(x => x.Id == request.RespostaPaiId.Value
                    && x.TopicoId == topicoId, cancellationToken);
            if (!paiExiste)
                return BadRequest(new { message = "Resposta pai inválida" });
        }

        var utilizador = await _dbContext.Utilizadores
            .FindAsync(new object[] { userId }, cancellationToken);
        if (utilizador is null)
            return Unauthorized(new { message = "Utilizador não encontrado" });

        var requereAprovacao = await _moderacaoService
            .RequereAprovacaoAsync(utilizador, cancellationToken);

        var resposta = new RespostaForum
        {
            TopicoId = topicoId,
            AutorId = userId,
            Conteudo = request.Conteudo.Trim(),
            RespostaPaiId = request.RespostaPaiId,
            DataCriacao = DateTime.UtcNow,
            EstadoResposta = requereAprovacao
                ? EstadoComentario.Pendente
                : EstadoComentario.Publicado
        };

        _dbContext.RespostasForum.Add(resposta);

        // Incrementa publicações do utilizador para controlo de moderação
        utilizador.NumeroPublicacoes++;

        await _dbContext.SaveChangesAsync(cancellationToken);

        // Notifica autor do tópico se não for ele próprio a responder
        if (topico.AutorId != userId)
        {
            await _notificacaoService.EnviarPushAsync(
                topico.AutorId,
                "Nova resposta no teu tópico",
                $"{utilizador.Nome} respondeu a \"{topico.Titulo}\"",
                cancellationToken);
        }

        await _dbContext.Entry(resposta)
            .Reference(r => r.Autor)
            .LoadAsync(cancellationToken);

        return Created(
            string.Empty,
            MapResposta(resposta, Array.Empty<RespostaForumDto>()));
    }

    [HttpPut("respostas/{id:int}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RespostaForumDto>> EditarResposta(
        int id,
        [FromBody] AtualizarRespostaForumDto request,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized(new { message = "Utilizador não autenticado" });

        if (string.IsNullOrWhiteSpace(request.Conteudo))
            return BadRequest(new { message = "Conteúdo é obrigatório" });

        var resposta = await _dbContext.RespostasForum
            .Include(x => x.Autor)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (resposta is null)
            return NotFound(new { message = "Resposta não encontrada" });

        if (resposta.AutorId != userId && !IsModerator())
            return Forbid();

        resposta.Conteudo = request.Conteudo.Trim();
        resposta.DataEdicao = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Ok(MapResposta(resposta, Array.Empty<RespostaForumDto>()));
    }

    [HttpDelete("respostas/{id:int}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ApagarResposta(
        int id,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized(new { message = "Utilizador não autenticado" });

        var resposta = await _dbContext.RespostasForum
            .FindAsync(new object[] { id }, cancellationToken);
        if (resposta is null)
            return NotFound(new { message = "Resposta não encontrada" });

        if (resposta.AutorId != userId && !IsModerator())
            return Forbid();

        resposta.EstadoResposta = EstadoComentario.Removido;
        await _dbContext.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    // ─────────────────────────────────────────
    // REACÇÕES
    // ─────────────────────────────────────────

    [HttpPost("reacoes")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<object>> ToggleReacao(
        [FromBody] CriarReacaoDto request,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized(new { message = "Utilizador não autenticado" });

        if (!HasExactlyOneTarget(request.TopicoForumId, request.RespostaForumId))
            return BadRequest(new { message = "Indique topicoForumId ou respostaForumId, mas não ambos" });

        if (string.IsNullOrWhiteSpace(request.Emoji))
            return BadRequest(new { message = "Emoji é obrigatório" });

        var reacaoExistente = await _dbContext.Reacoes
            .FirstOrDefaultAsync(x =>
                x.UtilizadorId == userId
                && x.TopicoForumId == request.TopicoForumId
                && x.RespostaForumId == request.RespostaForumId
                && x.Emoji == request.Emoji,
            cancellationToken);

        if (reacaoExistente is not null)
        {
            _dbContext.Reacoes.Remove(reacaoExistente);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Ok(new { adicionada = false });
        }

        _dbContext.Reacoes.Add(new Reacao
        {
            UtilizadorId = userId,
            TopicoForumId = request.TopicoForumId,
            RespostaForumId = request.RespostaForumId,
            Emoji = request.Emoji,
            CriadaEm = DateTime.UtcNow
        });

        await _dbContext.SaveChangesAsync(cancellationToken);
        return Ok(new { adicionada = true });
    }

    // ─────────────────────────────────────────
    // DENÚNCIAS
    // ─────────────────────────────────────────

    [HttpPost("denuncias")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<object>> Denunciar(
        [FromBody] CriarDenunciaDto request,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized(new { message = "Utilizador não autenticado" });

        if (!HasExactlyOneTarget(request.TopicoForumId, request.RespostaForumId))
            return BadRequest(new { message = "Indique topicoForumId ou respostaForumId, mas não ambos" });

        // Evita denúncia duplicada do mesmo utilizador
        var jaDenunciou = await _dbContext.Denuncias
            .AnyAsync(x =>
                x.UtilizadorId == userId
                && x.TopicoForumId == request.TopicoForumId
                && x.RespostaForumId == request.RespostaForumId,
            cancellationToken);

        if (jaDenunciou)
            return BadRequest(new { message = "Já denunciaste este conteúdo" });

        var tipoAlvo = request.TopicoForumId.HasValue
            ? TipoAlvoModeracao.Topico
            : TipoAlvoModeracao.Comentario;

        var idAlvo = request.TopicoForumId ?? request.RespostaForumId ?? 0;

        var denuncia = new DenunciaConteudo
        {
            UtilizadorId = userId,
            TopicoForumId = request.TopicoForumId,
            RespostaForumId = request.RespostaForumId,
            Motivo = request.Motivo,
            Descricao = request.Descricao,
            DataDenuncia = DateTime.UtcNow,
            TipoAlvo = tipoAlvo,
            IdAlvo = idAlvo,
            Estado = EstadoDenuncia.Pendente
        };

        _dbContext.Denuncias.Add(denuncia);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var suspendido = await _moderacaoService
            .ProcessarDenunciaAsync(denuncia, cancellationToken);

        return Ok(new { suspendido });
    }

    // ─────────────────────────────────────────
    // HELPERS PRIVADOS
    // ─────────────────────────────────────────

    private bool TryGetUserId(out int userId)
    {
        var value = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                 ?? User.FindFirst("sub")?.Value;
        return int.TryParse(value, out userId);
    }

    private bool IsModerator() =>
        User.IsInRole("Admin")
        || User.IsInRole("Moderador")
        || User.IsInRole("SuperAdmin");

    private static bool HasExactlyOneTarget(int? topicoId, int? respostaId) =>
        topicoId.HasValue ^ respostaId.HasValue;

    private static TopicoForumDto MapTopico(TopicoForum topico) =>
        new(
            topico.Id,
            topico.Titulo,
            topico.Descricao,
            topico.CategoriaId,
            topico.Categoria!.Nome,
            topico.AutorId,
            topico.Autor?.Nome,
            topico.Estado,
            topico.CriadoEm,
            topico.Fixado,
            topico.Visualizacoes);

    private static IReadOnlyCollection<RespostaForumDto> BuildRespostaTree(
        IReadOnlyCollection<RespostaForum> respostas,
        int? respostaPaiId,
        int profundidadeRestante)
    {
        if (profundidadeRestante <= 0)
            return Array.Empty<RespostaForumDto>();

        return respostas
            .Where(x => x.RespostaPaiId == respostaPaiId)
            .OrderBy(x => x.DataCriacao)
            .Select(x => MapResposta(
                x,
                BuildRespostaTree(respostas, x.Id, profundidadeRestante - 1)))
            .ToList();
    }

    private static RespostaForumDto MapResposta(
        RespostaForum resposta,
        IReadOnlyCollection<RespostaForumDto> filhas) =>
        new(
            resposta.Id,
            resposta.Conteudo,
            resposta.AutorId,
            resposta.Autor?.Nome,
            resposta.EstadoResposta,
            resposta.DataCriacao,
            resposta.DataEdicao,
            resposta.RespostaPaiId,
            resposta.IsSolucao,
            filhas);
}