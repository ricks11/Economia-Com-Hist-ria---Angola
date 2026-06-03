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

    public async Task<TopicoForum?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.TopicosForum
            .Include(x => x.Autor)
            .Include(x => x.Categoria)
            .Include(x => x.Respostas)
                .ThenInclude(x => x.Autor)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<TopicoForum>> GetAllAprovadosAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.TopicosForum
            .Include(x => x.Autor)
            .Include(x => x.Categoria)
            .Where(x => x.Estado == EstadoTopicoForum.Ativo)
            .OrderByDescending(x => x.CriadoEm)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<TopicoForum>> GetPendentesAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.TopicosForum
            .Include(x => x.Autor)
            .Include(x => x.Categoria)
            .Where(x => x.Estado == EstadoTopicoForum.Pendente)
            .OrderBy(x => x.CriadoEm)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<TopicoForum>> GetByCategoriaAsync(
        int categoriaId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.TopicosForum
            .Include(x => x.Autor)
            .Include(x => x.Categoria)
            .Where(x => x.CategoriaId == categoriaId
                && x.Estado == EstadoTopicoForum.Ativo)
            .OrderByDescending(x => x.CriadoEm)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<TopicoForum> AddAsync(
        TopicoForum topico,
        CancellationToken cancellationToken = default)
    {
        _dbContext.TopicosForum.Add(topico);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return topico;
    }

    public async Task<TopicoForum> UpdateAsync(
        TopicoForum topico,
        CancellationToken cancellationToken = default)
    {
        _dbContext.TopicosForum.Update(topico);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return topico;
    }

    public async Task UpdateEstadoAsync(
        int id,
        EstadoTopicoForum estado,
        CancellationToken cancellationToken = default)
    {
        var topico = await _dbContext.TopicosForum
            .FindAsync(new object[] { id }, cancellationToken);

        if (topico is null)
            return;

        topico.Estado = estado;
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}