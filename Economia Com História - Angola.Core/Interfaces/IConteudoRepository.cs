using EconomiaComHistoria.Core.Entities;
using EconomiaComHistoria.Core.Enums;

namespace EconomiaComHistoria.Core.Interfaces;

public interface IConteudoRepository
{
    Task<Conteudo?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<IEnumerable<Conteudo>> GetAllWithFiltersAsync(
    string? tema = null,
    NivelDificuldade? nivel = null,
    string? regiao = null,
    TipoConteudo? tipo = null,
    int? pageNumber = null,
    int? pageSize = null,
    CancellationToken cancellationToken = default);

    Task<int> GetCountWithFiltersAsync(
        string? tema = null,
        NivelDificuldade? nivel = null,
        string? regiao = null,
        TipoConteudo? tipo = null,
        CancellationToken cancellationToken = default);

    Task<Conteudo> AddAsync(Conteudo conteudo, CancellationToken cancellationToken = default);

    Task<Conteudo> UpdateAsync(Conteudo conteudo, CancellationToken cancellationToken = default);

    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
