using EconomiaComHistoria.Core.Entities;
using EconomiaComHistoria.Core.Enums;
using EconomiaComHistoria.Core.Interfaces;
using EconomiaComHistoria.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EconomiaComHistoria.Infrastructure.Services;

public class ModeracaoService : IModeracaoService
{
    private readonly AppDbContext _dbContext;

    public ModeracaoService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> RequereAprovacaoAsync(Utilizador utilizador, CancellationToken cancellationToken = default)
    {
        var totalTopicos = await _dbContext.TopicosForum
            .CountAsync(x => x.AutorId == utilizador.Id, cancellationToken);
        var totalRespostas = await _dbContext.RespostasForum
            .CountAsync(x => x.AutorId == utilizador.Id, cancellationToken);

        return totalTopicos + totalRespostas < 5;
    }

    public async Task<bool> ProcessarDenunciaAsync(
    DenunciaConteudo denuncia,
    CancellationToken cancellationToken = default)
    {
        // Caso 1: denúncia num tópico
        if (denuncia.TopicoForumId is not null)
        {
            var topico = await _dbContext.TopicosForum
                .FirstOrDefaultAsync(x => x.Id == denuncia.TopicoForumId.Value, cancellationToken);
            if (topico is null) return false;

            var totalDenuncias = await _dbContext.Denuncias
                .CountAsync(x => x.TopicoForumId == topico.Id, cancellationToken);

            if (totalDenuncias >= 3)
            {
                topico.Estado = EstadoTopicoForum.Suspenso;
                await _dbContext.SaveChangesAsync(cancellationToken);
                return true;
            }

            return false;
        }

        // Caso 2: denúncia numa resposta
        if (denuncia.RespostaForumId is not null)
        {
            var resposta = await _dbContext.RespostasForum
                .FirstOrDefaultAsync(x => x.Id == denuncia.RespostaForumId.Value, cancellationToken);
            if (resposta is null) return false;

            var totalDenuncias = await _dbContext.Denuncias
                .CountAsync(x => x.RespostaForumId == resposta.Id, cancellationToken);

            if (totalDenuncias >= 3)
            {
                resposta.EstadoResposta = EstadoComentario.Removido;
                await _dbContext.SaveChangesAsync(cancellationToken);
                return true;
            }

            return false;
        }

        return false;
    }
}
