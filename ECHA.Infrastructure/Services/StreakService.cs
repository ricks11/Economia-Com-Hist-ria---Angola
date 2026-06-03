using EconomiaComHistoria.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EconomiaComHistoria.Infrastructure.Services;

public interface IStreakService
{
    Task AtualizarStreakAsync(int utilizadorId, CancellationToken ct = default);
}

public class StreakService : IStreakService
{
    private readonly AppDbContext _context;

    public StreakService(AppDbContext context)
    {
        _context = context;
    }

    public async Task AtualizarStreakAsync(int utilizadorId, CancellationToken ct = default)
    {
        var user = await _context.Utilizadores.FindAsync(new object[] { utilizadorId }, ct);
        if (user == null) return;

        var hoje = DateTime.UtcNow.Date;
        var ultimoAcesso = user.UltimoAcesso?.Date;

        if (ultimoAcesso == hoje)
        {
            // Já acessou hoje, nada a fazer
            return;
        }

        if (ultimoAcesso == hoje.AddDays(-1))
        {
            // Acessou ontem, incrementa streak
            user.StreakAtual++;
        }
        else
        {
            // Quebrou o streak
            user.StreakAtual = 1;
        }

        user.UltimoAcesso = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);
    }
}
