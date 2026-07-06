using EconomiaComHistoria.Core.Entities;
using EconomiaComHistoria.Core.Interfaces;
using EconomiaComHistoria.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EconomiaComHistoria.Infrastructure.Repositories;

public class EventoGamificacaoRepository : BaseRepository<EventoGamificacao>, IEventoGamificacaoRepository
{
    public EventoGamificacaoRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<EventoGamificacao>> GetByUtilizadorAsync(int utilizadorId)
        => await _dbSet.Where(e => e.UtilizadorId == utilizadorId)
            .OrderByDescending(e => e.DataEvento)
            .ToListAsync();
}
