using EconomiaComHistoria.Core.DTOs;
using EconomiaComHistoria.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ECHA.API.Controllers;

/// <summary>
/// Progresso do utilizador no mapa de Angola por província.
/// Mapeia conteúdos vistos por região (campo Regiao) ao código ISO da província.
/// </summary>
[ApiController]
[Route("api/mapa")]
[Authorize]
public class MapaController : ControllerBase
{
    private readonly AppDbContext _db;

    // Mapeamento de nomes de região (campo Regiao dos Conteudos) → código ISO da província
    private static readonly Dictionary<string, string> RegiaoParaProvinciaId = new(StringComparer.OrdinalIgnoreCase)
    {
        { "Bengo",            "AO-BGO" },
        { "Benguela",         "AO-BGU" },
        { "Bié",              "AO-BIE" },
        { "Cabinda",          "AO-CAB" },
        { "Cuando-Cubango",   "AO-CCU" },
        { "Cuanza Norte",     "AO-CNO" },
        { "Cuanza Sul",       "AO-CUS" },
        { "Cunene",           "AO-CNN" },
        { "Huambo",           "AO-HUA" },
        { "Huíla",            "AO-HUI" },
        { "Luanda",           "AO-LUA" },
        { "Lunda Norte",      "AO-LNO" },
        { "Lunda Sul",        "AO-LSU" },
        { "Malanje",          "AO-MAL" },
        { "Moxico",           "AO-MOX" },
        { "Namibe",           "AO-NAM" },
        { "Uíge",             "AO-UIG" },
        { "Zaire",            "AO-ZAI" },
    };

    public MapaController(AppDbContext db) => _db = db;

    private bool TryGetUserId(out int userId)
    {
        var val = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
               ?? User.FindFirst("sub")?.Value;
        return int.TryParse(val, out userId);
    }

    /// <summary>
    /// GET api/mapa/progresso
    /// Retorna o percentual de conteúdos vistos por província para o utilizador autenticado.
    /// </summary>
    [HttpGet("progresso")]
    [ProducesResponseType(typeof(MapaProgressoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<MapaProgressoDto>> GetProgresso(CancellationToken ct)
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized(new { message = "Utilizador não autenticado." });

        // Total de conteúdos publicados por região
        var totalPorRegiao = await _db.Conteudos
            .Where(c => c.Estado == EconomiaComHistoria.Core.Enums.EstadoConteudo.Publicado)
            .GroupBy(c => c.Regiao)
            .Select(g => new { Regiao = g.Key, Total = g.Count() })
            .AsNoTracking()
            .ToListAsync(ct);

        // Conteúdos vistos pelo utilizador, agrupados por região
        var vistosPorRegiao = await _db.VisualizacoesConteudo
            .Where(cv => cv.UtilizadorId == userId)
            .Include(cv => cv.Conteudo)
            .GroupBy(cv => cv.Conteudo!.Regiao)
            .Select(g => new { Regiao = g.Key, Vistos = g.Count() })
            .AsNoTracking()
            .ToListAsync(ct);

        var visitosDict = vistosPorRegiao.ToDictionary(v => v.Regiao, v => v.Vistos, StringComparer.OrdinalIgnoreCase);

        var provincias = new List<ProvinciaProgressoDto>();

        foreach (var (nome, id) in RegiaoParaProvinciaId)
        {
            var totalRegiao = totalPorRegiao.FirstOrDefault(t => string.Equals(t.Regiao, nome, StringComparison.OrdinalIgnoreCase))?.Total ?? 0;
            var vistosRegiao = visitosDict.TryGetValue(nome, out var v) ? v : 0;
            var percentual = totalRegiao > 0 ? (double)vistosRegiao / totalRegiao : 0.0;

            provincias.Add(new ProvinciaProgressoDto(id, nome, percentual, vistosRegiao, totalRegiao));
        }

        return Ok(new MapaProgressoDto(provincias));
    }
}
