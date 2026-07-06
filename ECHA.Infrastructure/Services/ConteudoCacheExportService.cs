using EconomiaComHistoria.Core.DTOs.Sync;
using EconomiaComHistoria.Core.Interfaces;
using EconomiaComHistoria.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EconomiaComHistoria.Infrastructure.Services;

public class ConteudoCacheExportService : IConteudoCacheExportService
{
    private readonly AppDbContext _context;

    public ConteudoCacheExportService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ConteudoOfflinePacoteDto?> ExportarParaCacheAsync(int conteudoId)
    {
        var conteudo = await _context.Conteudos
            .Include(c => c.Traducoes)
            .FirstOrDefaultAsync(c => c.Id == conteudoId);

        if (conteudo == null) return null;

        return new ConteudoOfflinePacoteDto(
            conteudo.Id,
            conteudo.Titulo,
            conteudo.Resumo,
            conteudo.Tipo.ToString(),
            conteudo.Tema,
            conteudo.VideoUrl,
            conteudo.ThumbnailUrl,
            conteudo.DuracaoMinutos,
            conteudo.Nivel.ToString(),
            conteudo.IsJindungo,
            conteudo.ReferenciaFactual,
            conteudo.Traducoes.Select(t =>
                new ConteudoTraducaoOfflineDto(t.Lingua.ToString(), t.TextoTraduzido, t.AudioUrl)).ToList(),
            DateTime.UtcNow
        );
    }
}
