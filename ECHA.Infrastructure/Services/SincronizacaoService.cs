using EconomiaComHistoria.Core.DTOs.Sync;
using EconomiaComHistoria.Core.Entities;
using EconomiaComHistoria.Core.Enums;
using EconomiaComHistoria.Core.Interfaces;
using EconomiaComHistoria.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EconomiaComHistoria.Infrastructure.Services;

public class SincronizacaoService : ISincronizacaoService
{
    private readonly AppDbContext _context;
    private readonly IValidadorSincronizacao _validador;

    public SincronizacaoService(AppDbContext context, IValidadorSincronizacao validador)
    {
        _context = context;
        _validador = validador;
    }

    public async Task<LoteSincronizacaoResponse> ProcessarLoteAsync(int utilizadorId, LoteSincronizacaoRequest request)
    {
        var resultados = new List<ResultadoSincronizacaoItem>();
        var agoraUtc = DateTime.UtcNow;

        foreach (var item in request.Tentativas)
        {
            // 1. Idempotência: já processámos este IdLocal antes?
            var jaExiste = await _context.TentativasQuiz
                .FirstOrDefaultAsync(t => t.IdLocal == item.IdLocal && t.UtilizadorId == utilizadorId);

            if (jaExiste != null)
            {
                resultados.Add(new ResultadoSincronizacaoItem(
                    item.IdLocal, jaExiste.Id, true, jaExiste.ElegivelRanking, null));
                continue;
            }

            // 2. O quiz existe e está ativo?
            var quiz = await _context.Quizzes.FindAsync(item.QuizId);
            if (quiz == null || !quiz.Ativo)
            {
                resultados.Add(new ResultadoSincronizacaoItem(
                    item.IdLocal, null, false, false, "quiz_inativo"));
                continue;
            }

            // 3. Validação temporal (a regra que pediste)
            var validacaoTemporal = _validador.ValidarTimestamp(item.DataRealizacaoCliente, agoraUtc);
            bool elegivelRanking = validacaoTemporal.DentroDaTolerancia;

            // 4. Se está dentro da tolerância, ainda falta verificar RN-13 (24h entre tentativas)
            if (elegivelRanking)
            {
                var ultimaTentativaElegivel = await _context.TentativasQuiz
                    .Where(t => t.UtilizadorId == utilizadorId
                             && t.QuizId == item.QuizId
                             && t.ElegivelRanking)
                    .OrderByDescending(t => t.DataHora)
                    .FirstOrDefaultAsync();

                if (ultimaTentativaElegivel != null &&
                    !_validador.RespeitaIntervaloRanking(ultimaTentativaElegivel.DataHora, item.DataRealizacaoCliente))
                {
                    elegivelRanking = false;
                }
            }

            // 5. Criar e persistir a tentativa — NUNCA rejeitamos por causa do timestamp
            var tentativa = new TentativaQuiz
            {
                IdLocal = item.IdLocal,
                UtilizadorId = utilizadorId,
                QuizId = item.QuizId,
                DataHora = item.DataRealizacaoCliente,
                TempoGastoSeg = item.TempoGastoSeg,
                Origem = OrigemTentativa.SincronizadoOffline,
                ElegivelRanking = elegivelRanking
            };

            _context.TentativasQuiz.Add(tentativa);
            await _context.SaveChangesAsync(); // guarda item a item — um item mau não deita o lote todo fora

            resultados.Add(new ResultadoSincronizacaoItem(
                item.IdLocal, tentativa.Id, true, elegivelRanking, null));
        }

        return new LoteSincronizacaoResponse(resultados);
    }
}
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