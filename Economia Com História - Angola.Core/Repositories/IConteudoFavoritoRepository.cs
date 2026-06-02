using EconomiaComHistoria.Core.Entities;

namespace EconomiaComHistoria.Core.Repositories;

public interface IConteudoFavoritoRepository
{
    Task<ConteudoFavorito?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<IEnumerable<ConteudoFavorito>> GetByUtilizadorIdAsync(
        int utilizadorId,
        CancellationToken cancellationToken = default);

    Task<ConteudoFavorito?> GetByConteudoAndUtilizadorAsync(
        int conteudoId,
        int utilizadorId,
        CancellationToken cancellationToken = default);

    Task<ConteudoFavorito> AddAsync(ConteudoFavorito favorito, CancellationToken cancellationToken = default);

    Task RemoveAsync(int id, CancellationToken cancellationToken = default);

    Task RemoveByConteudoAndUtilizadorAsync(
        int conteudoId,
        int utilizadorId,
        CancellationToken cancellationToken = default);
}
