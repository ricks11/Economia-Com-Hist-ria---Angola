using EconomiaComHistoria.Core.Entities;
using EconomiaComHistoria.Core.DTOs;
using EconomiaComHistoria.Core.Enums;
using EconomiaComHistoria.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EconomiaComHistoria.Infrastructure.Services;

public interface IRelatorioService
{
    Task<RelatorioStatusDto> SolicitarRelatorioAsync(int utilizadorId, SolicitarRelatorioDto request, CancellationToken ct = default);
    Task<RelatorioStatusDto?> GetStatusAsync(int id, CancellationToken ct = default);
}

public class RelatorioService : IRelatorioService
{
    private readonly AppDbContext _context;

    public RelatorioService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<RelatorioStatusDto> SolicitarRelatorioAsync(int utilizadorId, SolicitarRelatorioDto request, CancellationToken ct = default)
    {
        var relatorio = new RelatorioProgresso
        {
            Titulo = request.Titulo,
            Tipo = request.Tipo,
            UtilizadorId = utilizadorId,
            TurmaId = request.TurmaId,
            EscolaId = request.EscolaId,
            Estado = EstadoRelatorio.Pendente,
            DataSolicitacao = DateTime.UtcNow
        };

        _context.RelatoriosProgresso.Add(relatorio);
        await _context.SaveChangesAsync(ct);

        // In a real scenario, this would trigger a background job
        // For now, we return the pending status
        return MapToStatusDto(relatorio);
    }

    public async Task<RelatorioStatusDto?> GetStatusAsync(int id, CancellationToken ct = default)
    {
        var relatorio = await _context.RelatoriosProgresso.FindAsync(new object[] { id }, ct);
        if (relatorio == null) return null;

        return MapToStatusDto(relatorio);
    }

    private RelatorioStatusDto MapToStatusDto(RelatorioProgresso r)
    {
        return new RelatorioStatusDto(
            r.Id,
            r.Titulo,
            r.Estado,
            r.DataSolicitacao,
            r.DataConclusao,
            r.Estado == EstadoRelatorio.Concluido ? $"/api/relatorios/{r.Id}/download" : null
        );
    }
}
