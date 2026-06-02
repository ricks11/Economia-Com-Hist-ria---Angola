using EconomiaComHistoria.Core.Entities;
using EconomiaComHistoria.Core.Interfaces;
using EconomiaComHistoria.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EconomiaComHistoria.Infrastructure.Repositories;

public class ConteudoFavoritoRepository : IConteudoFavoritoRepository
{
    private readonly AppDbContext _dbContext;

    public ConteudoFavoritoRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ConteudoFavorito?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.ConteudosFavoritos
            .FirstOrDefaultAsync(f => f.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<ConteudoFavorito>> GetByUtilizadorIdAsync(
        int utilizadorId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.ConteudosFavoritos
            .Where(f => f.UtilizadorId == utilizadorId)
            .Include(f => f.Conteudo)
            .ThenInclude(c => c.Autor)
            .OrderByDescending(f => f.DataAdicionado)
            .ToListAsync(cancellationToken);
    }

    public async Task<ConteudoFavorito?> GetByConteudoAndUtilizadorAsync(
        int conteudoId,
        int utilizadorId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.ConteudosFavoritos
            .FirstOrDefaultAsync(f => f.ConteudoId == conteudoId && f.UtilizadorId == utilizadorId, cancellationToken);
    }

    public async Task<ConteudoFavorito> AddAsync(ConteudoFavorito favorito, CancellationToken cancellationToken = default)
    {
        _dbContext.ConteudosFavoritos.Add(favorito);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return favorito;
    }

    public async Task RemoveAsync(int id, CancellationToken cancellationToken = default)
    {
        var favorito = await _dbContext.ConteudosFavoritos.FindAsync(
            new object[] { id }, cancellationToken: cancellationToken);

        if (favorito != null)
        {
            _dbContext.ConteudosFavoritos.Remove(favorito);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task RemoveByConteudoAndUtilizadorAsync(
        int conteudoId,
        int utilizadorId,
        CancellationToken cancellationToken = default)
    {
        var favorito = await _dbContext.ConteudosFavoritos
            .FirstOrDefaultAsync(f => f.ConteudoId == conteudoId && f.UtilizadorId == utilizadorId, cancellationToken);

        if (favorito != null)
        {
            _dbContext.ConteudosFavoritos.Remove(favorito);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
