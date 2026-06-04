using EconomiaComHistoria.Core.Entities;
using EconomiaComHistoria.Core.Enums;

namespace EconomiaComHistoria.Core.Interfaces;

public interface ITopicoForumRepository
{
    Task<TopicoForum?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<TopicoForum>> GetAllAprovadosAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<TopicoForum>> GetPendentesAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<TopicoForum>> GetByCategoriaAsync(int categoriaId, CancellationToken cancellationToken = default);
    Task UpdateEstadoAsync(int id, EstadoTopicoForum estado, CancellationToken cancellationToken = default);
    Task<TopicoForum> AddAsync(TopicoForum topico, CancellationToken cancellationToken = default);
    Task<TopicoForum> UpdateAsync(TopicoForum topico, CancellationToken cancellationToken = default);
}
