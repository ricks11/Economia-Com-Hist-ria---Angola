using EconomiaComHistoria.Core.Entities;

namespace EconomiaComHistoria.Core.Interfaces;

public interface IDenunciaRepository
{
    Task<DenunciaConteudo> AddAsync(DenunciaConteudo denuncia, CancellationToken cancellationToken = default);
    Task<IEnumerable<DenunciaConteudo>> GetByTopicoIdAsync(int topicoId, CancellationToken cancellationToken = default);
    Task<int> CountByTopicoIdAsync(int topicoId, CancellationToken cancellationToken = default);
}
