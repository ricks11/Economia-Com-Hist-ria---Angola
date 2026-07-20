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
    private readonly IAuditoriaService _auditoriaService;

    public ForumController(
        AppDbContext dbContext,
        IModeracaoService moderacaoService,
        INotificacaoService notificacaoService,
        IAuditoriaService auditoriaService)
    {
        _dbContext = dbContext;
        _moderacaoService = moderacaoService;
        _notificacaoService = notificacaoService;
        _auditoriaService = auditoriaService;
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

        // --- VALIDAÇÕES DE VISIBILIDADE ---
        if (request.Visibilidade == Visibilidade.Escola && !request.EscolaId.HasValue)
            return BadRequest(new { message = "Visibilidade 'Escola' requer EscolaId." });
        if (request.Visibilidade == Visibilidade.Turma && !request.TurmaId.HasValue)
            return BadRequest(new { message = "Visibilidade 'Turma' requer TurmaId." });

        if (request.Visibilidade == Visibilidade.Escola && request.EscolaId != utilizador.EscolaId)
            return BadRequest(new { message = "Não pertence a esta escola." });
        if (request.Visibilidade == Visibilidade.Turma && request.TurmaId != utilizador.TurmaId)
            return BadRequest(new { message = "Não pertence a esta turma." });

        var requereAprovacao = await _moderacaoService
            .RequereAprovacaoAsync(utilizador, cancellationToken);

        var topico = new TopicoForum
        {
            Titulo = request.Titulo.Trim(),
            Descricao = request.Descricao.Trim(),
            CategoriaId = request.CategoriaId,
            AutorId = userId,
            Estado = requereAprovacao ? EstadoTopicoForum.Pendente : EstadoTopicoForum.Ativo,
            CriadoEm = DateTime.UtcNow,
            Visibilidade = request.Visibilidade,
            EscolaId = request.EscolaId,
            TurmaId = request.TurmaId
        };

        _dbContext.TopicosForum.Add(topico);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditoriaService.RegistarAsync(
            userId,
            "CriarTopico",
            "TopicoForum",
            topico.Id,
            null,
            $"Título: {topico.Titulo}, Categoria: {topico.CategoriaId}",
            HttpContext
        );

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
    public async Task<ActionResult<IEnumerable<CategoriaForumDto>>> ListarCategorias(
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

        return Ok(categorias.Select(c => new CategoriaForumDto(c.Id, c.Nome, c.Descricao, c.Icone)));
    }

    [HttpGet("topicos")]
    [AllowAnonymous]
    [ResponseCache(Duration = 120, VaryByQueryKeys = new[] { "categoriaId", "ordem", "incluirArquivados" })]
    public async Task<ActionResult<IEnumerable<TopicoForumDto>>> ListarTopicos(
        [FromQuery] int? categoriaId,
        [FromQuery] string? ordem,
        [FromQuery] bool incluirArquivados = false,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        var podeVerTodos = userId.HasValue && IsModerator();

        var query = _dbContext.TopicosForum
            .Include(x => x.Autor)
            .Include(x => x.Categoria)
            .Include(x => x.Escola)
            .Include(x => x.Turma)
            .AsNoTracking();

        // --- FILTRO POR VISIBILIDADE ---
        query = query.Where(x =>
            // Público
            x.Visibilidade == Visibilidade.Publico
            // Privado: autor ou moderador
            || (x.Visibilidade == Visibilidade.Privado && userId.HasValue && (x.AutorId == userId.Value || IsModerator()))
            // Escola: utilizador com a mesma escola
            || (x.Visibilidade == Visibilidade.Escola && userId.HasValue &&
                _dbContext.Utilizadores.Any(u => u.Id == userId.Value && u.EscolaId == x.EscolaId))
            // Turma: utilizador com a mesma turma
            || (x.Visibilidade == Visibilidade.Turma && userId.HasValue &&
                _dbContext.Utilizadores.Any(u => u.Id == userId.Value && u.TurmaId == x.TurmaId))
            // Moderadores veem tudo
            || (podeVerTodos)
        );

        // --- FILTRO POR ESTADO ---
        query = query.Where(x =>
            x.Estado == EstadoTopicoForum.Ativo
            || (userId.HasValue && x.AutorId == userId.Value && x.Estado == EstadoTopicoForum.Pendente)
            || (podeVerTodos && x.Estado == EstadoTopicoForum.Pendente)
            || (incluirArquivados && userId.HasValue &&
                (x.Estado == EstadoTopicoForum.Arquivado || x.Estado == EstadoTopicoForum.Rejeitado) &&
                (x.AutorId == userId.Value || IsModerator()))
        );

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
        var userId = GetUserId();

        var topico = await _dbContext.TopicosForum
            .Include(x => x.Autor)
            .Include(x => x.Categoria)
            .Include(x => x.Escola)
            .Include(x => x.Turma)
            .Include(x => x.Reacoes)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (topico is null)
            return NotFound(new { message = "Tópico não encontrado" });

        // --- VERIFICAÇÃO DE PERMISSÃO POR VISIBILIDADE ---
        bool podeVer = false;
        if (topico.Visibilidade == Visibilidade.Publico)
            podeVer = true;
        else if (topico.Visibilidade == Visibilidade.Privado && userId.HasValue)
            podeVer = topico.AutorId == userId.Value || IsModerator();
        else if (topico.Visibilidade == Visibilidade.Escola && userId.HasValue)
        {
            var utilizador = await _dbContext.Utilizadores.FindAsync(userId.Value);
            podeVer = (utilizador != null && topico.EscolaId == utilizador.EscolaId) || IsModerator();
        }
        else if (topico.Visibilidade == Visibilidade.Turma && userId.HasValue)
        {
            var utilizador = await _dbContext.Utilizadores.FindAsync(userId.Value);
            podeVer = (utilizador != null && topico.TurmaId == utilizador.TurmaId) || IsModerator();
        }

        if (!podeVer)
            return NotFound(new { message = "Tópico não encontrado" });

        // --- VERIFICAÇÃO DE ESTADO ---
        if (topico.Estado != EstadoTopicoForum.Ativo &&
            !(userId.HasValue && (topico.AutorId == userId.Value || IsModerator())))
            return NotFound(new { message = "Tópico não encontrado" });

        // --- INCREMENTAR VISUALIZAÇÕES (apenas se ativo) ---
        if (topico.Estado == EstadoTopicoForum.Ativo)
        {
            var topicoTracked = await _dbContext.TopicosForum.FindAsync(new object[] { id }, cancellationToken);
            if (topicoTracked is not null)
            {
                topicoTracked.Visualizacoes++;
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
        }

        // --- CARREGAR RESPOSTAS ---
        var respostas = await _dbContext.RespostasForum
            .Include(x => x.Autor)
            .Where(x => x.TopicoId == id && (x.EstadoResposta == EstadoComentario.Publicado || x.EstadoResposta == EstadoComentario.Aprovada))
            .OrderBy(x => x.DataCriacao)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        // --- CARREGAR REAÇÕES DAS RESPOSTAS ---
        var respostaIds = respostas.Select(r => r.Id).ToList();
        var reacoesDasRespostas = await _dbContext.Reacoes
            .Where(x => x.RespostaForumId.HasValue && respostaIds.Contains(x.RespostaForumId.Value))
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        // --- CÁLCULOS DE REAÇÃO DO TÓPICO ---
        var totalReacoesTopico = topico.Reacoes.Count;
        var jaReagiuAoTopico = userId.HasValue && topico.Reacoes.Any(r => r.UtilizadorId == userId.Value);

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
            jaReagiuAoTopico,
            totalReacoesTopico,
            BuildRespostaTree(respostas, reacoesDasRespostas, userId, null, 2),
            topico.Visibilidade,
            topico.EscolaId,
            topico.Escola?.Nome,
            topico.TurmaId,
            topico.Turma?.Nome
        ));
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

        await _auditoriaService.RegistarAsync(
            userId,
            "ArquivarTopico",
            "TopicoForum",
            id,
            null,
            "Arquivado",
            HttpContext
        );
        return NoContent();
    }

    [HttpPut("topicos/{id:int}/desarquivar")]
    [Authorize(Roles = "Admin,Editor,SuperAdmin")]
    public async Task<IActionResult> DesarquivarTopico(int id, CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized(new { message = "Utilizador não autenticado" });

        var topico = await _dbContext.TopicosForum.FindAsync(new object[] { id }, cancellationToken);
        if (topico is null)
            return NotFound(new { message = "Tópico não encontrado" });

        if (topico.Estado != EstadoTopicoForum.Arquivado)
            return BadRequest(new { message = "O tópico não está arquivado." });

        topico.Estado = EstadoTopicoForum.Ativo;
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditoriaService.RegistarAsync(
            userId,
            "DesarquivarTopico",
            "TopicoForum",
            id,
            "Arquivado",
            "Ativo",
            HttpContext
        );
        return NoContent();
    }

    // ─────────────────────────────────────────
    // RESPOSTAS
    // ─────────────────────────────────────────

    [HttpGet("respostas/{id:int}")]
    [AllowAnonymous]
    public async Task<ActionResult<RespostaForumDto>> GetResposta(int id, CancellationToken cancellationToken)
    {
        var resposta = await _dbContext.RespostasForum
            .Include(x => x.Autor)
            .FirstOrDefaultAsync(x => x.Id == id && x.EstadoResposta != EstadoComentario.Removido, cancellationToken);

        if (resposta is null)
            return NotFound(new { message = "Resposta não encontrada" });

        var reacoes = await _dbContext.Reacoes
            .Where(r => r.RespostaForumId == id)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var totalReacoes = reacoes.Count;
        var userId = GetUserId();
        var jaReagiu = userId.HasValue && reacoes.Any(r => r.UtilizadorId == userId.Value);

        return Ok(MapResposta(resposta, jaReagiu, totalReacoes, Array.Empty<RespostaForumDto>()));
    }

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

        utilizador.NumeroPublicacoes++;

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditoriaService.RegistarAsync(
            userId,
            "AdicionarResposta",
            "RespostaForum",
            resposta.Id,
            null,
            $"Tópico {topicoId}",
            HttpContext
        );

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
            MapResposta(resposta, false, 0, Array.Empty<RespostaForumDto>()));
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

        var reacoes = await _dbContext.Reacoes
            .Where(x => x.RespostaForumId == id)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var totalReacoes = reacoes.Count;
        var jaReagiu = reacoes.Any(x => x.UtilizadorId == userId);

        return Ok(MapResposta(resposta, jaReagiu, totalReacoes, Array.Empty<RespostaForumDto>()));
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

        await _auditoriaService.RegistarAsync(
            userId,
            "RemoverResposta",
            "RespostaForum",
            id,
            null,
            "Removido",
            HttpContext
        );
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

    private int? GetUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
        if (int.TryParse(claim, out var id)) return id;
        return null;
    }

    private bool TryGetUserId(out int userId)
    {
        var value = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                 ?? User.FindFirst("sub")?.Value;
        return int.TryParse(value, out userId);
    }

    private bool IsModerator() =>
        User.IsInRole("Admin")
        || User.IsInRole("Moderador")
        || User.IsInRole("SuperAdmin")
        || User.IsInRole("Editor");

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
            topico.Visualizacoes,
            topico.Visibilidade,
            topico.EscolaId,
            topico.Escola?.Nome,
            topico.TurmaId,
            topico.Turma?.Nome
        );

    private static IReadOnlyCollection<RespostaForumDto> BuildRespostaTree(
        IReadOnlyCollection<RespostaForum> respostas,
        IReadOnlyCollection<Reacao> reacoesDasRespostas,
        int? userId,
        int? respostaPaiId,
        int profundidadeRestante)
    {
        if (profundidadeRestante <= 0)
            return Array.Empty<RespostaForumDto>();

        return respostas
            .Where(x => x.RespostaPaiId == respostaPaiId)
            .OrderBy(x => x.DataCriacao)
            .Select(x => {
                var reacoesDestaResposta = reacoesDasRespostas.Where(r => r.RespostaForumId == x.Id).ToList();
                var totalReacoes = reacoesDestaResposta.Count;
                var jaReagiu = userId.HasValue && reacoesDestaResposta.Any(r => r.UtilizadorId == userId.Value);

                return MapResposta(
                    x,
                    jaReagiu,
                    totalReacoes,
                    BuildRespostaTree(respostas, reacoesDasRespostas, userId, x.Id, profundidadeRestante - 1));
            })
            .ToList();
    }

    private static RespostaForumDto MapResposta(
        RespostaForum resposta,
        bool jaReagiu,
        int totalReacoes,
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
            jaReagiu,
            totalReacoes,
            resposta.TopicoId,
            filhas
        );
}