using EconomiaComHistoria.Core.Entities;
using Microsoft.AspNetCore.Http;

namespace EconomiaComHistoria.Core.Interfaces;

public interface IAuditoriaService
{
    Task RegistarAsync(
        int utilizadorId,
        string acao,
        string recurso,
        int? idRecurso = null,
        string? dadosAntes = null,
        string? dadosDepois = null,
        HttpContext? httpContext = null);

    Task<IEnumerable<AuditoriaLog>> ObterLogs(int? utilizadorId = null, string? acao = null, DateTime? inicio = null, DateTime? fim = null);
}