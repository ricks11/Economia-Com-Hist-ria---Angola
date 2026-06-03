using EconomiaComHistoria.API.DTOs;
using EconomiaComHistoria.Core.Enums;
using EconomiaComHistoria.Core.Interfaces;
using EconomiaComHistoria.Infrastructure.Data;
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

    public ModeracaoController(AppDbContext dbContext, INotificacaoService notificacaoService)
    {
        _dbContext = dbContext;
        _notificacaoService = notificacaoService;
    }

    [HttpGet("pendentes")]
    public async Task<ActionResult<object>> GetPendentes(CancellationToken cancellationToken)
    {
        var topicos = await _dbContext.TopicosForum
            .Include(x => x.Autor)
            .Include(x => x.Categoria)
            .Where(x => x.Estado == EstadoTopicoForum.Pendente)
            .OrderBy(x => x.CriadoEm)
            .Select(x => new
            {
                x.Id,
                x.Titulo,
                x.AutorId,
                AutorNome = x.Autor == null ? null : x.Autor.Nome,
                CategoriaId = x.CategoriaId,
                CategoriaNome = x.Categoria == null ? null : x.Categoria.Nome,
                x.Descricao,
                x.CriadoEm
            })
            .ToListAsync(cancellationToken);

        var respostas = await _dbContext.RespostasForum
            .Include(x => x.Autor)
            .Where(x => x.EstadoResposta == EstadoComentario.Pendente)
            .OrderBy(x => x.DataCriacao)
            .Select(x => new
            {
                x.Id,
                x.TopicoId,
                x.AutorId,
                AutorNome = x.Autor == null ? null : x.Autor.Nome,
                x.RespostaPaiId,
                x.DataCriacao,
                x.Conteudo
            })
            .ToListAsync(cancellationToken);

        return Ok(new { topicos, respostas });
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

    [HttpGet("denuncias")]
    public async Task<ActionResult<IEnumerable<object>>> GetDenuncias(CancellationToken cancellationToken)
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
    public async Task<IActionResult> SuspenderUtilizador(int id, SuspenderUtilizadorDto request, CancellationToken cancellationToken)
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
}