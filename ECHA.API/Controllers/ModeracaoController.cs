using EconomiaComHistoria.Core.DTOs;
using EconomiaComHistoria.Core.Enums;
using EconomiaComHistoria.Core.Interfaces;
using EconomiaComHistoria.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EconomiaComHistoria.API.Controllers;

[ApiController]
[Authorize(Roles = "Admin,Editor")]
[Route("api/moderacao")]
public class ModeracaoController : ControllerBase
{
    private readonly AppDbContext _dbContext;
    private readonly INotificacaoService _notificacaoService;

    public ModeracaoController(AppDbContext dbContext, INotificacaoService notificacaoService)
    {
        _dbContext = dbContext;
        _notificacaoService = notificacaoService;
    }

    [HttpGet("pendentes")]
    public async Task<ActionResult<ModeracaoPendentesResponse>> GetPendentes(CancellationToken cancellationToken)
    {
        var topicos = await _dbContext.TopicosForum
            .Include(x => x.Autor)
            .Include(x => x.Categoria)
            .Where(x => x.EstadoTopico == EstadoTopico.Pendente)
            .OrderBy(x => x.DataCriacao)
            .Select(x => new ModeracaoPendenteDto(
                x.Id,
                "Topico",
                x.Titulo,
                x.AutorId,
                x.Autor == null ? null : x.Autor.Nome,
                x.DataCriacao,
                x.CategoriaId,
                x.Categoria == null ? null : x.Categoria.Nome,
                null,
                x.TotalDenuncias
            ))
            .ToListAsync(cancellationToken);

        var respostas = await _dbContext.RespostasForum
            .Include(x => x.Autor)
            .Where(x => x.EstadoResposta == EstadoResposta.Pendente)
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
                0 // Respostas no forum nao tem contador de denuncias direto no modelo atual
            ))
            .ToListAsync(cancellationToken);

        return Ok(new ModeracaoPendentesResponse(topicos, respostas));
    }

    [HttpGet("denuncias")]
    public async Task<ActionResult<IEnumerable<DenunciaSummaryDto>>> GetDenuncias(CancellationToken cancellationToken)
    {
        var topicos = await _dbContext.TopicosForum
            .Include(x => x.Autor)
            .Where(x => x.TotalDenuncias > 0)
            .OrderByDescending(x => x.TotalDenuncias)
            .Select(x => new DenunciaSummaryDto(
                x.Id,
                "Topico",
                x.Titulo,
                x.AutorId,
                x.Autor == null ? null : x.Autor.Nome,
                x.TotalDenuncias,
                DateTime.UtcNow // Idealmente teria data da ultima denuncia
            ))
            .ToListAsync(cancellationToken);

        return Ok(topicos);
    }

    [HttpGet("utilizadores")]
    public async Task<ActionResult<IEnumerable<UtilizadorModeracaoDto>>> ListUtilizadores(CancellationToken cancellationToken)
    {
        var utilizadores = await _dbContext.Utilizadores
            .OrderBy(x => x.Nome)
            .Select(x => new UtilizadorModeracaoDto(
                x.Id,
                x.Nome,
                x.Email,
                x.Tipo.ToString(),
                x.SuspensoAte.HasValue && x.SuspensoAte > DateTime.UtcNow,
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

        topico.EstadoTopico = EstadoTopico.Aprovado;
        topico.MotivoRejeicao = null;
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

        resposta.EstadoResposta = EstadoResposta.Aprovada;
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _notificacaoService.EnviarPushAsync(resposta.AutorId, "Resposta aprovada", "A sua resposta foi aprovada.", cancellationToken);
        return NoContent();
    }

    [HttpPut("topicos/{id:int}/rejeitar")]
    public async Task<IActionResult> RejeitarTopico(int id, RejeitarTopicoDto request, CancellationToken cancellationToken)
    {
        var topico = await _dbContext.TopicosForum.FindAsync(new object[] { id }, cancellationToken);
        if (topico is null)
            return NotFound(new { message = "Topico nao encontrado" });

        topico.EstadoTopico = EstadoTopico.Rejeitado;
        topico.MotivoRejeicao = request.MotivoRejeicao;
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _notificacaoService.EnviarPushAsync(topico.AutorId, "Topico rejeitado", request.MotivoRejeicao, cancellationToken);
        return NoContent();
    }

    [HttpPut("respostas/{id:int}/rejeitar")]
    public async Task<IActionResult> RejeitarResposta(int id, RejeitarTopicoDto request, CancellationToken cancellationToken)
    {
        var resposta = await _dbContext.RespostasForum.FindAsync(new object[] { id }, cancellationToken);
        if (resposta is null)
            return NotFound(new { message = "Resposta nao encontrada" });

        resposta.EstadoResposta = EstadoResposta.Rejeitada;
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _notificacaoService.EnviarPushAsync(resposta.AutorId, "Resposta rejeitada", request.MotivoRejeicao, cancellationToken);
        return NoContent();
    }

    [HttpPut("utilizadores/{id:int}/suspender")]
    public async Task<IActionResult> SuspenderUtilizador(int id, SuspenderUtilizadorDto request, CancellationToken cancellationToken)
    {
        var utilizador = await _dbContext.Utilizadores.FindAsync(new object[] { id }, cancellationToken);
        if (utilizador is null)
            return NotFound(new { message = "Utilizador nao encontrado" });

        utilizador.SuspensaoPermanente = !request.DiasSuspensao.HasValue;
        utilizador.SuspensoAte = request.DiasSuspensao.HasValue
            ? DateTime.UtcNow.AddDays(request.DiasSuspensao.Value)
            : null;

        await _dbContext.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpPut("utilizadores/{id:int}/reativar")]
    public async Task<IActionResult> ReativarUtilizador(int id, CancellationToken cancellationToken)
    {
        var utilizador = await _dbContext.Utilizadores.FindAsync(new object[] { id }, cancellationToken);
        if (utilizador is null)
            return NotFound(new { message = "Utilizador nao encontrado" });

        utilizador.SuspensaoPermanente = false;
        utilizador.SuspensoAte = null;

        await _dbContext.SaveChangesAsync(cancellationToken);
        return NoContent();
    }
}
