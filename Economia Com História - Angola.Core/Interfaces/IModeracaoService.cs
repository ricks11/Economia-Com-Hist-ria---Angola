using EconomiaComHistoria.Core.Entities;

namespace EconomiaComHistoria.Core.Interfaces;

public interface IModeracaoService
{
    Task<bool> RequereAprovacaoAsync(Utilizador utilizador, CancellationToken cancellationToken = default);
    Task<bool> ProcessarDenunciaAsync(DenunciaConteudo denuncia, CancellationToken cancellationToken = default);
}
