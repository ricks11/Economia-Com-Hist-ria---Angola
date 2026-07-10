using EconomiaComHistoria.Core.DTOs;
using EconomiaComHistoria.Core.Enums;
using EconomiaComHistoria.Core.Interfaces;
using EconomiaComHistoria.Infrastructure.Data;
using EconomiaComHistoria.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EconomiaComHistoria.API.Controllers;

[ApiController]
[Authorize(Roles = "Admin,Editor,Moderador,SuperAdmin")]
[Route("api/moderacao")]
public class ModeracaoController : ControllerBase
{
    private readonly AppDbContext _dbContext;
    private readonly INotificacaoService _notificacaoService;
    private readonly IGamificacaoService _gamificacaoService;

    public ModeracaoController(AppDbContext dbContext, INotificacaoService notificacaoService, IGamificacaoService gamificacaoService)
    {
        _dbContext = dbContext;
        _notificacaoService = notificacaoService;
        _gamificacaoService = gamificacaoService;
    }

    [HttpGet("pendentes")]
    public async Task<ActionResult<ModeracaoPendentesResponse>> GetPendentes(CancellationToken cancellationToken)
    {
        var topicos = await _dbContext.TopicosForum
            .Include(x => x.Autor)
            .Include(x => x.Categoria)
            .Where(x => x.Estado == EstadoTopicoForum.Pendente)
            .OrderBy(x => x.CriadoEm)
            .Select(x => new ModeracaoPendenteDto(
                x.Id,
                "Topico",
                x.Titulo,
                x.AutorId,
                x.Autor == null ? null : x.Autor.Nome,
                x.CriadoEm,
                x.CategoriaId,
                x.Categoria == null ? null : x.Categoria.Nome,
                null,
                x.Denuncias.Count
            ))
            .ToListAsync(cancellationToken);

        var respostas = await _dbContext.RespostasForum
            .Include(x => x.Autor)
            .Where(x => x.EstadoResposta == EstadoComentario.Pendente)
            .OrderBy(x => x.DataCriacao)
            .Select(x => new ModeracaoPendenteDto(
                x.Id,
                "Resposta",
                x.Conteudo,
                x.AutorId,
                x.Autor == null ? null : x.Autor.Nome,
                x.DataCriacao,
                null,
                null,
                x.TopicoId,
                x.Denuncias.Count
            ))
            .ToListAsync(cancellationToken);

        return Ok(new ModeracaoPendentesResponse(topicos, respostas));
    }

    [HttpGet("denuncias")]
    public async Task<ActionResult<IEnumerable<DenunciaSummaryDto>>> GetDenuncias(CancellationToken cancellationToken)
    {
        var topicos = await _dbContext.TopicosForum
            .Include(x => x.Autor)
            .Where(x => x.Denuncias.Any())
            .OrderByDescending(x => x.Denuncias.Count)
            .Select(x => new DenunciaSummaryDto(
                x.Id,
                "Topico",
                x.Titulo,
                x.AutorId,
                x.Autor == null ? null : x.Autor.Nome,
                x.Denuncias.Count,
                x.Denuncias.Max(d => d.DataDenuncia)
            ))
            .ToListAsync(cancellationToken);

        return Ok(topicos);
    }

    [HttpGet("utilizadores")]
    public async Task<ActionResult<IEnumerable<UtilizadorModeracaoDto>>> GetUtilizadores(CancellationToken cancellationToken)
    {
        var utilizadores = await _dbContext.Utilizadores
            .Select(x => new UtilizadorModeracaoDto(
                x.Id,
                x.Nome,
                x.Email,
                x.Tipo.ToString(),
                x.Suspenso,
                x.SuspensoAte,
                x.SuspensaoPermanente
            ))
            .ToListAsync(cancellationToken);

        return Ok(utilizadores);
    }

    [HttpPut("topicos/{id:int}/aprovar")]
    public async Task<IActionResult> AprovarTopico(int id, CancellationToken cancellationToken)
    {
        var topico = await _dbContext.TopicosForum.FindAsync(new object[] { id }, cancellationToken);
        if (topico is null)
            return NotFound(new { message = "Topico nao encontrado" });

        topico.Estado = EstadoTopicoForum.Ativo;
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _notificacaoService.EnviarPushAsync(topico.AutorId, "Topico aprovado", $"O topico \"{topico.Titulo}\" foi aprovado.", cancellationToken);
        return NoContent();
    }

    [HttpPut("respostas/{id:int}/aprovar")]
    public async Task<IActionResult> AprovarResposta(int id, CancellationToken cancellationToken)
    {
        var resposta = await _dbContext.RespostasForum.FindAsync(new object[] { id }, cancellationToken);
        if (resposta is null)
            return NotFound(new { message = "Resposta nao encontrada" });

        resposta.EstadoResposta = EstadoComentario.Aprovada;
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _notificacaoService.EnviarPushAsync(resposta.AutorId, "Resposta aprovada", "A sua resposta foi aprovada.", cancellationToken);
        return NoContent();
    }

    [HttpPut("topicos/{id:int}/rejeitar")]
    public async Task<IActionResult> RejeitarTopico(int id, [FromBody] RejeitarTopicoDto request, CancellationToken cancellationToken)
    {
        var topico = await _dbContext.TopicosForum.FindAsync(new object[] { id }, cancellationToken);
        if (topico is null)
            return NotFound(new { message = "Topico nao encontrado" });

        topico.Estado = EstadoTopicoForum.Rejeitado;
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _notificacaoService.EnviarPushAsync(topico.AutorId, "Topico rejeitado", request.MotivoRejeicao, cancellationToken);
        return NoContent();
    }

    [HttpPut("respostas/{id:int}/rejeitar")]
    public async Task<IActionResult> RejeitarResposta(int id, [FromBody] RejeitarTopicoDto request, CancellationToken cancellationToken)
    {
        var resposta = await _dbContext.RespostasForum.FindAsync(new object[] { id }, cancellationToken);
        if (resposta is null)
            return NotFound(new { message = "Resposta nao encontrada" });

        resposta.EstadoResposta = EstadoComentario.Removido;
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _notificacaoService.EnviarPushAsync(resposta.AutorId, "Resposta rejeitada", request.MotivoRejeicao, cancellationToken);
        return NoContent();
    }

    [HttpGet("denuncias/detalhado")]
    public async Task<ActionResult> GetDenunciasDetalhado(CancellationToken cancellationToken)
    {
        var denuncias = await _dbContext.Denuncias
            .Include(x => x.TopicoForum)
                .ThenInclude(t => t!.Autor)
            .Include(x => x.RespostaForum)
                .ThenInclude(c => c!.Autor)
            .OrderByDescending(x => x.DataDenuncia)
            .Select(x => new
            {
                x.Id,
                x.TipoAlvo,
                x.IdAlvo,
                x.Motivo,
                x.Descricao,
                x.Estado,
                x.DataDenuncia,
                Topico = x.TopicoForum != null ? new
                {
                    x.TopicoForum.Id,
                    x.TopicoForum.Titulo,
                    AutorNome = x.TopicoForum.Autor != null ? x.TopicoForum.Autor.Nome : null
                } : null,
                Resposta = x.RespostaForum != null ? new
                {
                    x.RespostaForum.Id,
                    x.RespostaForum.Conteudo,
                    AutorNome = x.RespostaForum.Autor != null ? x.RespostaForum.Autor.Nome : null
                } : null
            })
            .ToListAsync(cancellationToken);

        return Ok(denuncias);
    }

    [HttpPut("utilizadores/{id:int}/suspender")]
    public async Task<IActionResult> SuspenderUtilizador(int id, [FromBody] SuspenderUtilizadorDto request, CancellationToken cancellationToken)
    {
        var utilizador = await _dbContext.Utilizadores.FindAsync(new object[] { id }, cancellationToken);
        if (utilizador is null)
            return NotFound(new { message = "Utilizador nao encontrado" });

        utilizador.SuspensoAte = request.DiasSuspensao.HasValue
            ? DateTime.UtcNow.AddDays(request.DiasSuspensao.Value)
            : DateTime.UtcNow.AddYears(100);

        await _dbContext.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpPut("utilizadores/{id:int}/reativar")]
    public async Task<IActionResult> ReativarUtilizador(int id, CancellationToken cancellationToken)
    {
        var utilizador = await _dbContext.Utilizadores.FindAsync(new object[] { id }, cancellationToken);
        if (utilizador is null)
            return NotFound(new { message = "Utilizador nao encontrado" });

        utilizador.SuspensoAte = null;

        await _dbContext.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpGet("badges")]
    public async Task<ActionResult<List<BadgeAdminDto>>> GetBadges(CancellationToken cancellationToken)
    {
        var badges = await _dbContext.Badges
            .Include(b => b.Conquistado)
            .Select(b => new BadgeAdminDto(
                b.Id,
                b.Nome,
                b.Descricao,
                b.IconeUrl,
                b.CriterioTipo,
                b.CriterioValor,
                b.Conquistado.Count
            ))
            .ToListAsync(cancellationToken);
        return Ok(badges);
    }

    [HttpGet("metricas-engajamento")]
    public async Task<ActionResult<object>> GetMetricasEngajamento(CancellationToken cancellationToken)
    {
        var metricas = await _gamificacaoService.GetMetricasEngajamentoAsync(cancellationToken);
        return Ok(metricas);
    }
}
