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

    public async Task<bool> ProcessarDenunciaAsync(DenunciaConteudo denuncia, CancellationToken cancellationToken = default)
    {
        if (denuncia.TopicoId is null)
            return false;

        var topico = await _dbContext.TopicosForum
            .FirstOrDefaultAsync(x => x.Id == denuncia.TopicoId.Value, cancellationToken);
        if (topico is null)
            return false;

        topico.TotalDenuncias = await _dbContext.DenunciasConteudo
            .CountAsync(x => x.TopicoId == topico.Id, cancellationToken);

        if (topico.TotalDenuncias >= 3)
        {
            topico.EstadoTopico = EstadoTopico.Suspenso;
            await _dbContext.SaveChangesAsync(cancellationToken);
            return true;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return false;
    }
}
