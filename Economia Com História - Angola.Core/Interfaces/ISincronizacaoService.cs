using EconomiaComHistoria.Core.DTOs.Sync;

namespace EconomiaComHistoria.Core.Interfaces;

public interface ISincronizacaoService
{
    Task<LoteSincronizacaoResponse> ProcessarLoteAsync(int utilizadorId, LoteSincronizacaoRequest request);
}

public interface IConteudoCacheExportService
{
    Task<ConteudoOfflinePacoteDto?> ExportarParaCacheAsync(int conteudoId);
}