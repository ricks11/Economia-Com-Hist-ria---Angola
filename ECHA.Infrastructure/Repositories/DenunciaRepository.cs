using EconomiaComHistoria.Core.Entities;
using EconomiaComHistoria.Core.Interfaces;
using EconomiaComHistoria.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EconomiaComHistoria.Infrastructure.Repositories;

public class DenunciaRepository : IDenunciaRepository
{
    private readonly AppDbContext _dbContext;

    public DenunciaRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<DenunciaConteudo> AddAsync(DenunciaConteudo denuncia, CancellationToken cancellationToken = default)
    {
        _dbContext.DenunciasConteudo.Add(denuncia);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return denuncia;
    }

    public async Task<IEnumerable<DenunciaConteudo>> GetByTopicoIdAsync(int topicoId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.DenunciasConteudo
            .Include(x => x.Denunciante)
            .Where(x => x.TopicoId == topicoId)
            .OrderByDescending(x => x.DataDenuncia)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountByTopicoIdAsync(int topicoId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.DenunciasConteudo.CountAsync(x => x.TopicoId == topicoId, cancellationToken);
    }
}
