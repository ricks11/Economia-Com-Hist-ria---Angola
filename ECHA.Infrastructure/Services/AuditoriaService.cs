using EconomiaComHistoria.Core.Entities;
using EconomiaComHistoria.Core.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Linq.Expressions;

namespace EconomiaComHistoria.Infrastructure.Services;

public class AuditoriaService : IAuditoriaService
{
    private readonly IAuditoriaLogRepository _auditoriaRepo;

    public AuditoriaService(IAuditoriaLogRepository auditoriaRepo)
    {
        _auditoriaRepo = auditoriaRepo;
    }

    public async Task RegistarAsync(
        int utilizadorId,
        string acao,
        string recurso,
        int? idRecurso = null,
        string? dadosAntes = null,
        string? dadosDepois = null,
        HttpContext? httpContext = null)
    {
        string? sessionId = null;
        if (httpContext != null)
        {
            try
            {
                sessionId = httpContext.Session.Id;
            }
            catch (InvalidOperationException)
            {
                // Session is not configured, so we leave sessionId as null.
            }
        }

        var log = new AuditoriaLog
        {
            UtilizadorId = utilizadorId,
            Acao = acao,
            Recurso = recurso,
            IdRecurso = idRecurso.GetValueOrDefault(),
            DadosAntes = dadosAntes,
            DadosDepois = dadosDepois,
            Ip = httpContext?.Connection.RemoteIpAddress?.ToString(),
            UserAgent = httpContext?.Request.Headers["User-Agent"].ToString(),
            Sessao = sessionId
        };

        await _auditoriaRepo.AddAsync(log);
    }

    public async Task<IEnumerable<AuditoriaLog>> ObterLogs(
        int? utilizadorId = null,
        string? acao = null,
        DateTime? inicio = null,
        DateTime? fim = null)
    {
        // Build expression for filtering
        Expression<Func<AuditoriaLog, bool>> filter = null;
        if (utilizadorId.HasValue || !string.IsNullOrEmpty(acao) || inicio.HasValue || fim.HasValue)
        {
            filter = log =>
                (!utilizadorId.HasValue || log.UtilizadorId == utilizadorId.Value) &&
                (string.IsNullOrEmpty(acao) || log.Acao == acao) &&
                (!inicio.HasValue || log.Timestamp >= inicio.Value) &&
                (!fim.HasValue || log.Timestamp <= fim.Value);
        }

        IEnumerable<AuditoriaLog> logs;
        if (filter != null)
            logs = await _auditoriaRepo.FindAsync(filter);
        else
            logs = await _auditoriaRepo.GetAllAsync();

        return logs.OrderByDescending(l => l.Timestamp);
    }
}
