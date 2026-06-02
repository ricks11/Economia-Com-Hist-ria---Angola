using EconomiaComHistoria.Core.Entities;
using EconomiaComHistoria.Core.Interfaces;
using EconomiaComHistoria.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EconomiaComHistoria.Infrastructure.Repositories;

public class ConteudoRepository : IConteudoRepository
{
    private readonly AppDbContext _dbContext;

    public ConteudoRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Conteudo?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Conteudos
            .Include(c => c.Autor)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Conteudo>> GetAllWithFiltersAsync(
        string? tema = null,
        string? nivel = null,
        string? regiao = null,
        string? tipo = null,
        int? pageNumber = null,
        int? pageSize = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Conteudos
            .Include(c => c.Autor)
            .AsNoTracking();

        // Apply filters
        if (!string.IsNullOrEmpty(tema))
            query = query.Where(c => c.Tema == tema);

        if (!string.IsNullOrEmpty(nivel))
            query = query.Where(c => c.Nivel == nivel);

        if (!string.IsNullOrEmpty(regiao))
            query = query.Where(c => c.Regiao == regiao);

        if (!string.IsNullOrEmpty(tipo))
            query = query.Where(c => c.Tipo == tipo);

        // Apply pagination
        if (pageNumber.HasValue && pageSize.HasValue && pageNumber > 0 && pageSize > 0)
        {
            query = query
                .OrderByDescending(c => c.DataPublicacao)
                .Skip((pageNumber.Value - 1) * pageSize.Value)
                .Take(pageSize.Value);
        }
        else
        {
            query = query.OrderByDescending(c => c.DataPublicacao);
        }

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<int> GetCountWithFiltersAsync(
        string? tema = null,
        string? nivel = null,
        string? regiao = null,
        string? tipo = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Conteudos.AsQueryable();

        // Apply filters
        if (!string.IsNullOrEmpty(tema))
            query = query.Where(c => c.Tema == tema);

        if (!string.IsNullOrEmpty(nivel))
            query = query.Where(c => c.Nivel == nivel);

        if (!string.IsNullOrEmpty(regiao))
            query = query.Where(c => c.Regiao == regiao);

        if (!string.IsNullOrEmpty(tipo))
            query = query.Where(c => c.Tipo == tipo);

        return await query.CountAsync(cancellationToken);
    }

    public async Task<Conteudo> AddAsync(Conteudo conteudo, CancellationToken cancellationToken = default)
    {
        _dbContext.Conteudos.Add(conteudo);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return conteudo;
    }

    public async Task<Conteudo> UpdateAsync(Conteudo conteudo, CancellationToken cancellationToken = default)
    {
        _dbContext.Conteudos.Update(conteudo);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return conteudo;
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var conteudo = await _dbContext.Conteudos.FindAsync(new object[] { id }, cancellationToken: cancellationToken);
        if (conteudo != null)
        {
            _dbContext.Conteudos.Remove(conteudo);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
