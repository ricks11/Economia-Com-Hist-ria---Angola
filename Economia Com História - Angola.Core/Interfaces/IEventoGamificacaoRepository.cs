using EconomiaComHistoria.Core.Entities;

namespace EconomiaComHistoria.Core.Interfaces;

public interface IEventoGamificacaoRepository : IRepository<EventoGamificacao>
{
    Task<IEnumerable<EventoGamificacao>> GetByUtilizadorAsync(int utilizadorId);
}