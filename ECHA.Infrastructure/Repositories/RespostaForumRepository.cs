using EconomiaComHistoria.Core.Entities;
using EconomiaComHistoria.Core.Enums;
using EconomiaComHistoria.Core.Interfaces;
using EconomiaComHistoria.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EconomiaComHistoria.Infrastructure.Repositories;

public class RespostaForumRepository : IRespostaForumRepository
{
    private readonly AppDbContext _dbContext;

    public RespostaForumRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<RespostaForum>> GetByTopicoAsync(int topicoId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.RespostasForum
            .Include(x => x.Autor)
            .Include(x => x.RespostasFilhas)
            .ThenInclude(x => x.Autor)
            .Where(x => x.TopicoId == topicoId && x.EstadoResposta == EstadoResposta.Aprovado)
            .OrderBy(x => x.DataCriacao)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<RespostaForum> AddAsync(RespostaForum resposta, CancellationToken cancellationToken = default)
    {
        _dbContext.RespostasForum.Add(resposta);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return resposta;
    }

    public async Task UpdateEstadoAsync(int id, EstadoResposta estado, CancellationToken cancellationToken = default)
    {
        var resposta = await _dbContext.RespostasForum.FindAsync(new object[] { id }, cancellationToken);
        if (resposta is null)
            return;

        resposta.EstadoResposta = estado;
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
