using EconomiaComHistoria.Core.Entities;
using EconomiaComHistoria.Core.Enums;

namespace EconomiaComHistoria.Core.Interfaces;

public interface IRespostaForumRepository
{
    Task<IEnumerable<RespostaForum>> GetByTopicoAsync(int topicoId, CancellationToken cancellationToken = default);
    Task<RespostaForum> AddAsync(RespostaForum resposta, CancellationToken cancellationToken = default);
    Task UpdateEstadoAsync(int id, EstadoResposta estado, CancellationToken cancellationToken = default);
}
