using System.Security.Claims;
using EconomiaComHistoria.API.DTOs;
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

    [HttpPost("topicos")]
    [Authorize]
    public async Task<ActionResult<TopicoForumDto>> CriarTopico(CriarTopicoForumDto request, CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized(new { message = "Utilizador nao autenticado" });

        if (string.IsNullOrWhiteSpace(request.Titulo) || string.IsNullOrWhiteSpace(request.Conteudo))
            return BadRequest(new { message = "Titulo e conteudo sao obrigatorios" });

        var utilizador = await _dbContext.Utilizadores.FindAsync(new object[] { userId }, cancellationToken);
        if (utilizador is null)
            return Unauthorized(new { message = "Utilizador nao encontrado" });

        var categoriaExiste = await _dbContext.CategoriasForum.AnyAsync(x => x.Id == request.CategoriaId, cancellationToken);
        if (!categoriaExiste)
            return BadRequest(new { message = "Categoria invalida" });

        var topico = new TopicoForum
        {
            Titulo = request.Titulo.Trim(),
            Conteudo = request.Conteudo.Trim(),
            AutorId = userId,
            CategoriaId = request.CategoriaId,
            DataCriacao = DateTime.UtcNow,
            EstadoTopico = await _moderacaoService.RequereAprovacaoAsync(utilizador, cancellationToken)
                ? EstadoTopico.Pendente
                : EstadoTopico.Aprovado
        };

        _dbContext.TopicosForum.Add(topico);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(GetTopico), new { id = topico.Id }, MapTopico(topico));
    }

    [HttpGet("topicos")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<TopicoForumDto>>> ListarTopicos(
        [FromQuery] int? categoriaId,
        [FromQuery] string? ordem,
        CancellationToken cancellationToken)
    {
        var query = _dbContext.TopicosForum
            .Include(x => x.Autor)
            .Include(x => x.Categoria)
            .Include(x => x.Respostas)
            .Where(x => x.EstadoTopico == EstadoTopico.Aprovado)
            .AsNoTracking();

        if (categoriaId.HasValue)
            query = query.Where(x => x.CategoriaId == categoriaId.Value);

        query = ordem?.Equals("activo", StringComparison.OrdinalIgnoreCase) == true
            ? query.OrderByDescending(x => x.Respostas.Max(r => (DateTime?)r.DataCriacao) ?? x.DataCriacao)
            : query.OrderByDescending(x => x.DataCriacao);

        var topicos = await query.ToListAsync(cancellationToken);
        return Ok(topicos.Select(MapTopico));
    }

    [HttpGet("topicos/{id:int}")]
    [AllowAnonymous]
    public async Task<ActionResult<TopicoForumDetalheDto>> GetTopico(int id, CancellationToken cancellationToken)
    {
        var topico = await _dbContext.TopicosForum
            .Include(x => x.Autor)
            .Include(x => x.Categoria)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && x.EstadoTopico == EstadoTopico.Aprovado, cancellationToken);

        if (topico is null)
            return NotFound(new { message = "Topico nao encontrado" });

        var respostas = await _dbContext.RespostasForum
            .Include(x => x.Autor)
            .Where(x => x.TopicoId == id && x.EstadoResposta == EstadoResposta.Aprovado)
            .OrderBy(x => x.DataCriacao)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return Ok(new TopicoForumDetalheDto(
            topico.Id,
            topico.Titulo,
            topico.Conteudo,
            topico.AutorId,
            topico.Autor?.Nome,
            topico.CategoriaId,
            topico.Categoria?.Nome,
            topico.EstadoTopico,
            topico.DataCriacao,
            topico.TotalDenuncias,
            BuildRespostaTree(respostas, null, 2)));
    }

    [HttpDelete("topicos/{id:int}")]
    [Authorize]
    public async Task<IActionResult> ApagarTopico(int id, CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized(new { message = "Utilizador nao autenticado" });

        var topico = await _dbContext.TopicosForum.FindAsync(new object[] { id }, cancellationToken);
        if (topico is null)
            return NotFound(new { message = "Topico nao encontrado" });

        if (topico.AutorId != userId && !IsModerator())
            return Forbid();

        topico.EstadoTopico = EstadoTopico.Suspenso;
        await _dbContext.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpPost("topicos/{id:int}/respostas")]
    [Authorize]
    public async Task<ActionResult<RespostaForumDto>> AdicionarResposta(int id, CriarRespostaForumDto request, CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized(new { message = "Utilizador nao autenticado" });

        if (string.IsNullOrWhiteSpace(request.Conteudo))
            return BadRequest(new { message = "Conteudo e obrigatorio" });

        var topico = await _dbContext.TopicosForum.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (topico is null || topico.EstadoTopico != EstadoTopico.Aprovado)
            return NotFound(new { message = "Topico nao encontrado" });

        if (request.RespostaPaiId.HasValue)
        {
            var paiExiste = await _dbContext.RespostasForum.AnyAsync(
                x => x.Id == request.RespostaPaiId.Value && x.TopicoId == id,
                cancellationToken);
            if (!paiExiste)
                return BadRequest(new { message = "Resposta pai invalida" });
        }

        var utilizador = await _dbContext.Utilizadores.FindAsync(new object[] { userId }, cancellationToken);
        if (utilizador is null)
            return Unauthorized(new { message = "Utilizador nao encontrado" });

        var resposta = new RespostaForum
        {
            TopicoId = id,
            AutorId = userId,
            Conteudo = request.Conteudo.Trim(),
            RespostaPaiId = request.RespostaPaiId,
            DataCriacao = DateTime.UtcNow,
            EstadoResposta = await _moderacaoService.RequereAprovacaoAsync(utilizador, cancellationToken)
                ? EstadoResposta.Pendente
                : EstadoResposta.Aprovado
        };

        _dbContext.RespostasForum.Add(resposta);
        await _dbContext.SaveChangesAsync(cancellationToken);

        if (topico.AutorId != userId)
        {
            await _notificacaoService.EnviarPushAsync(
                topico.AutorId,
                "Nova resposta no topico",
                $"O seu topico \"{topico.Titulo}\" recebeu uma resposta.",
                cancellationToken);
        }

        return Created(string.Empty, MapResposta(resposta, Array.Empty<RespostaForumDto>()));
    }

    [HttpPut("respostas/{id:int}")]
    [Authorize]
    public async Task<ActionResult<RespostaForumDto>> EditarResposta(int id, AtualizarRespostaForumDto request, CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized(new { message = "Utilizador nao autenticado" });

        var resposta = await _dbContext.RespostasForum.Include(x => x.Autor).FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (resposta is null)
            return NotFound(new { message = "Resposta nao encontrada" });

        if (resposta.AutorId != userId && !IsModerator())
            return Forbid();

        if (string.IsNullOrWhiteSpace(request.Conteudo))
            return BadRequest(new { message = "Conteudo e obrigatorio" });

        resposta.Conteudo = request.Conteudo.Trim();
        resposta.DataEdicao = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Ok(MapResposta(resposta, Array.Empty<RespostaForumDto>()));
    }

    [HttpDelete("respostas/{id:int}")]
    [Authorize]
    public async Task<IActionResult> ApagarResposta(int id, CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized(new { message = "Utilizador nao autenticado" });

        var resposta = await _dbContext.RespostasForum.FindAsync(new object[] { id }, cancellationToken);
        if (resposta is null)
            return NotFound(new { message = "Resposta nao encontrada" });

        if (resposta.AutorId != userId && !IsModerator())
            return Forbid();

        resposta.EstadoResposta = EstadoResposta.Suspenso;
        await _dbContext.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpPost("reacoes")]
    [Authorize]
    public async Task<ActionResult<object>> ToggleReacao(CriarReacaoDto request, CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized(new { message = "Utilizador nao autenticado" });

        if (!HasExactlyOneTarget(request.TopicoId, request.RespostaId))
            return BadRequest(new { message = "Informe topicoId ou respostaId, mas nao ambos" });

        var reacao = await _dbContext.Reacoes.FirstOrDefaultAsync(
            x => x.UtilizadorId == userId
                && x.TopicoId == request.TopicoId
                && x.RespostaId == request.RespostaId
                && x.TipoReacao == request.TipoReacao,
            cancellationToken);

        if (reacao is not null)
        {
            _dbContext.Reacoes.Remove(reacao);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Ok(new { adicionada = false });
        }

        _dbContext.Reacoes.Add(new Reacao
        {
            UtilizadorId = userId,
            TopicoId = request.TopicoId,
            RespostaId = request.RespostaId,
            TipoReacao = request.TipoReacao
        });
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Ok(new { adicionada = true });
    }

    [HttpPost("denuncias")]
    [Authorize]
    public async Task<ActionResult<object>> Denunciar(CriarDenunciaDto request, CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized(new { message = "Utilizador nao autenticado" });

        if (!HasExactlyOneTarget(request.TopicoId, request.RespostaId))
            return BadRequest(new { message = "Informe topicoId ou respostaId, mas nao ambos" });

        if (string.IsNullOrWhiteSpace(request.Motivo))
            return BadRequest(new { message = "Motivo e obrigatorio" });

        var denuncia = new DenunciaConteudo
        {
            DenuncianteId = userId,
            TopicoId = request.TopicoId,
            RespostaId = request.RespostaId,
            Motivo = request.Motivo.Trim(),
            DataDenuncia = DateTime.UtcNow,
            DataCriacao = DateTime.UtcNow
        };

        _dbContext.DenunciasConteudo.Add(denuncia);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var suspendido = await _moderacaoService.ProcessarDenunciaAsync(denuncia, cancellationToken);
        return Ok(new { suspendido });
    }

    private bool TryGetUserId(out int userId)
    {
        var value = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
        return int.TryParse(value, out userId);
    }

    private bool IsModerator()
    {
        return User.IsInRole("Admin") || User.IsInRole("Editor") || User.IsInRole("Moderador") || User.IsInRole("SuperAdmin");
    }

    private static bool HasExactlyOneTarget(int? topicoId, int? respostaId)
    {
        return topicoId.HasValue ^ respostaId.HasValue;
    }

    private static TopicoForumDto MapTopico(TopicoForum topico)
    {
        return new TopicoForumDto(
            topico.Id,
            topico.Titulo,
            topico.Conteudo,
            topico.AutorId,
            topico.Autor?.Nome,
            topico.CategoriaId,
            topico.Categoria?.Nome,
            topico.EstadoTopico,
            topico.DataCriacao,
            topico.TotalDenuncias);
    }

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
            .Select(x => MapResposta(x, BuildRespostaTree(respostas, x.Id, profundidadeRestante - 1)))
            .ToList();
    }

    private static RespostaForumDto MapResposta(RespostaForum resposta, IReadOnlyCollection<RespostaForumDto> respostas)
    {
        return new RespostaForumDto(
            resposta.Id,
            resposta.TopicoId,
            resposta.AutorId,
            resposta.Autor?.Nome,
            resposta.Conteudo,
            resposta.RespostaPaiId,
            resposta.EstadoResposta,
            resposta.DataCriacao,
            resposta.DataEdicao,
            respostas);
    }
}
