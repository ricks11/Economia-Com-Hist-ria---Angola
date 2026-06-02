using EconomiaComHistoria.Core.Entities;
using EconomiaComHistoria.Core.Enums;
using EconomiaComHistoria.Core.Interfaces;
using EconomiaComHistoria.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EconomiaComHistoria.Infrastructure.Repositories;

public class TopicoForumRepository : ITopicoForumRepository
{
    private readonly AppDbContext _dbContext;

    public TopicoForumRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<TopicoForum?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.TopicosForum
            .Include(x => x.Autor)
            .Include(x => x.Categoria)
            .Include(x => x.Respostas)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<TopicoForum>> GetAllAprovadosAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.TopicosForum
            .Include(x => x.Autor)
            .Include(x => x.Categoria)
            .Where(x => x.EstadoTopico == EstadoTopico.Aprovado)
            .OrderByDescending(x => x.DataCriacao)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<TopicoForum>> GetPendentesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.TopicosForum
            .Include(x => x.Autor)
            .Include(x => x.Categoria)
            .Where(x => x.EstadoTopico == EstadoTopico.Pendente)
            .OrderBy(x => x.DataCriacao)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<TopicoForum>> GetByCategoriaAsync(int categoriaId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.TopicosForum
            .Include(x => x.Autor)
            .Include(x => x.Categoria)
            .Where(x => x.CategoriaId == categoriaId && x.EstadoTopico == EstadoTopico.Aprovado)
            .OrderByDescending(x => x.DataCriacao)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task UpdateEstadoAsync(int id, EstadoTopico estado, CancellationToken cancellationToken = default)
    {
        var topico = await _dbContext.TopicosForum.FindAsync(new object[] { id }, cancellationToken);
        if (topico is null)
            return;

        topico.EstadoTopico = estado;
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
