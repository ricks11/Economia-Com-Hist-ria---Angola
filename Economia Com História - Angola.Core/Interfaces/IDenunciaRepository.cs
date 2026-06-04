using EconomiaComHistoria.Core.Entities;
using EconomiaComHistoria.Core.Enums;

namespace EconomiaComHistoria.Core.Interfaces;

public interface IDenunciaRepository
{
    Task<DenunciaConteudo> AddAsync(DenunciaConteudo denuncia, CancellationToken cancellationToken = default);
    Task<IEnumerable<DenunciaConteudo>> GetByTopicoIdAsync(int topicoId, CancellationToken cancellationToken = default);
    Task<int> CountByTopicoIdAsync(int topicoId, CancellationToken cancellationToken = default);
    Task<DenunciaConteudo?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<DenunciaConteudo>> GetByRespostaIdAsync(int respostaId, CancellationToken cancellationToken = default);
    Task<int> CountByRespostaIdAsync(int respostaId, CancellationToken cancellationToken = default);
    Task<bool> JaDenunciouAsync(int utilizadorId, TipoAlvoModeracao tipo, int idAlvo, CancellationToken cancellationToken = default);
}
