using EconomiaComHistoria.Core.Entities;
using EconomiaComHistoria.Core.Enums;
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
        _dbContext.Denuncias.Add(denuncia);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return denuncia;
    }

    public async Task<IEnumerable<DenunciaConteudo>> GetByTopicoIdAsync(int topicoId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Denuncias
            .Include(x => x.Utilizador)
            .Where(x => x.TopicoForumId == topicoId)
            .OrderByDescending(x => x.DataDenuncia)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountByTopicoIdAsync(int topicoId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Denuncias.CountAsync(x => x.TopicoForumId == topicoId, cancellationToken);
    }

    public async Task<DenunciaConteudo?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Denuncias
            .Include(x => x.Utilizador)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<DenunciaConteudo>> GetByRespostaIdAsync(int respostaId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Denuncias
            .Include(x => x.Utilizador)
            .Where(x => x.RespostaForumId == respostaId)
            .OrderByDescending(x => x.DataDenuncia)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountByRespostaIdAsync(int respostaId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Denuncias
            .CountAsync(x => x.RespostaForumId == respostaId, cancellationToken);
    }

    public async Task<bool> JaDenunciouAsync(int utilizadorId, TipoAlvoModeracao tipo, int idAlvo, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Denuncias
            .AnyAsync(x => x.UtilizadorId == utilizadorId
                && x.TipoAlvo == tipo
                && x.IdAlvo == idAlvo, cancellationToken);
    }
}
