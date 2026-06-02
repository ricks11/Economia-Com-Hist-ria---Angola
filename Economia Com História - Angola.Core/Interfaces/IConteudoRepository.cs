using EconomiaComHistoria.Core.Entities;

namespace EconomiaComHistoria.Core.Interfaces;

public interface IConteudoRepository
{
    Task<Conteudo?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<IEnumerable<Conteudo>> GetAllWithFiltersAsync(
        string? tema = null,
        string? nivel = null,
        string? regiao = null,
        string? tipo = null,
        int? pageNumber = null,
        int? pageSize = null,
        CancellationToken cancellationToken = default);

    Task<int> GetCountWithFiltersAsync(
        string? tema = null,
        string? nivel = null,
        string? regiao = null,
        string? tipo = null,
        CancellationToken cancellationToken = default);

    Task<Conteudo> AddAsync(Conteudo conteudo, CancellationToken cancellationToken = default);

    Task<Conteudo> UpdateAsync(Conteudo conteudo, CancellationToken cancellationToken = default);

    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
