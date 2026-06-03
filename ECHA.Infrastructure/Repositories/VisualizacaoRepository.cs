using EconomiaComHistoria.Core.Entities;
using EconomiaComHistoria.Core.Interfaces;
using EconomiaComHistoria.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EconomiaComHistoria.Infrastructure.Repositories;

public class VisualizacaoRepository : IVisualizacaoRepository
{
    private readonly AppDbContext _dbContext;

    public VisualizacaoRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<VisualizacaoConteudo?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Visualizacoes
            .FirstOrDefaultAsync(v => v.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<VisualizacaoConteudo>> GetByConteudoIdAsync(
        int conteudoId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Visualizacoes
            .Where(v => v.ConteudoId == conteudoId)
            .OrderByDescending(v => v.DataHora)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<VisualizacaoConteudo>> GetByUtilizadorIdAsync(
        int utilizadorId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Visualizacoes
            .Where(v => v.UtilizadorId == utilizadorId)
            .OrderByDescending(v => v.DataHora)
            .ToListAsync(cancellationToken);
    }

    public async Task<VisualizacaoConteudo?> GetByConteudoAndUtilizadorAsync(
        int conteudoId,
        int utilizadorId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Visualizacoes
            .FirstOrDefaultAsync(v => v.ConteudoId == conteudoId && v.UtilizadorId == utilizadorId, cancellationToken);
    }

    public async Task<VisualizacaoConteudo> AddAsync(VisualizacaoConteudo visualizacao, CancellationToken cancellationToken = default)
    {
        _dbContext.Visualizacoes.Add(visualizacao);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return visualizacao;
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var visualizacao = await _dbContext.Visualizacoes.FindAsync(
            new object[] { id }, cancellationToken: cancellationToken);

        if (visualizacao != null)
        {
            _dbContext.Visualizacoes.Remove(visualizacao);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
