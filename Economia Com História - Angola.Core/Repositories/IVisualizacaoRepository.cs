using EconomiaComHistoria.Core.Entities;

namespace EconomiaComHistoria.Core.Repositories;

public interface IVisualizacaoRepository
{
    Task<VisualizacaoConteudo?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<IEnumerable<VisualizacaoConteudo>> GetByConteudoIdAsync(
        int conteudoId,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<VisualizacaoConteudo>> GetByUtilizadorIdAsync(
        int utilizadorId,
        CancellationToken cancellationToken = default);

    Task<VisualizacaoConteudo?> GetByConteudoAndUtilizadorAsync(
        int conteudoId,
        int utilizadorId,
        CancellationToken cancellationToken = default);

    Task<VisualizacaoConteudo> AddAsync(VisualizacaoConteudo visualizacao, CancellationToken cancellationToken = default);

    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
