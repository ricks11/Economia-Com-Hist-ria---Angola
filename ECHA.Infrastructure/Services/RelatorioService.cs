using EconomiaComHistoria.Core.Entities;
using EconomiaComHistoria.Core.DTOs;
using EconomiaComHistoria.Core.Enums;
using EconomiaComHistoria.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EconomiaComHistoria.Infrastructure.Services;

public interface IRelatorioService
{
    Task<RelatorioStatusDto> SolicitarRelatorioAsync(int utilizadorId, SolicitarRelatorioDto request, CancellationToken ct = default);
    Task<RelatorioStatusDto?> GetStatusAsync(int id, CancellationToken ct = default);
    Task<List<RelatorioListaDto>> ListarRelatoriosAsync(int utilizadorId, int? escolaId = null, CancellationToken ct = default);
    Task<byte[]?> DownloadRelatorioAsync(int id, CancellationToken ct = default);
}

public class RelatorioService : IRelatorioService
{
    private readonly AppDbContext _context;
    private readonly ILogger<RelatorioService> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    public RelatorioService(AppDbContext context, ILogger<RelatorioService> logger, IServiceScopeFactory scopeFactory)
    {
        _context = context;
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    public async Task<RelatorioStatusDto> SolicitarRelatorioAsync(int utilizadorId, SolicitarRelatorioDto request, CancellationToken ct = default)
    {
        var relatorio = new RelatorioProgresso
        {
            Titulo = request.Titulo,
            Tipo = "CSV", // Sempre CSV pois é o único formato que realmente geramos
            UtilizadorId = utilizadorId,
            TurmaId = request.TurmaId,
            EscolaId = request.EscolaId,
            Estado = EstadoRelatorio.Pendente,
            DataSolicitacao = DateTime.UtcNow,
            UrlDownload = null,
            DataInicio = request.Inicio,
            DataFim = request.Fim
        };

        _context.RelatoriosProgresso.Add(relatorio);
        await _context.SaveChangesAsync(ct);

        _ = Task.Run(async () => await ProcessarRelatorioAsync(relatorio.Id));

        return MapToStatusDto(relatorio);
    }

    private async Task ProcessarRelatorioAsync(int relatorioId)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var relatorio = await dbContext.RelatoriosProgresso.FindAsync(relatorioId);
            if (relatorio == null) return;

            relatorio.Estado = EstadoRelatorio.Processando;
            await dbContext.SaveChangesAsync();

            // Simular processamento
            await Task.Delay(1000);

            // Gerar relatório com base nos parâmetros do relatório
            var fileName = $"relatorio_{relatorio.Id}_{DateTime.Now:yyyyMMddHHmmss}.csv";
            var filePath = Path.Combine(Path.GetTempPath(), fileName);

            // Cabeçalho do CSV
            var dados = new List<string> { "ID,Nome,Email,Escola,Turma,Data de Registo" };

            // Consulta base para utilizadores
            var usersQuery = dbContext.Utilizadores
                .Include(u => u.Escola)
                .Include(u => u.Turma)
                .AsQueryable();

            // Aplicar filtros baseado nos parâmetros do relatório
            if (relatorio.EscolaId.HasValue)
            {
                usersQuery = usersQuery.Where(u => u.EscolaId == relatorio.EscolaId.Value);
            }

            if (relatorio.TurmaId.HasValue)
            {
                usersQuery = usersQuery.Where(u => u.TurmaId == relatorio.TurmaId.Value);
            }

            // Filtro por período de data
            if (relatorio.DataInicio.HasValue)
            {
                usersQuery = usersQuery.Where(u => u.DataRegisto >= relatorio.DataInicio.Value);
            }

            if (relatorio.DataFim.HasValue)
            {
                usersQuery = usersQuery.Where(u => u.DataRegisto <= relatorio.DataFim.Value);
            }

            // Executar consulta e adicionar resultados ao CSV
            var users = await usersQuery.ToListAsync();
            foreach (var user in users)
            {
                var escolaNome = user.Escola?.Nome ?? "N/A";
                var turmaNome = user.Turma?.Nome ?? "N/A";
                var dataRegisto = user.DataRegisto.ToString("yyyy-MM-dd HH:mm:ss");

                // Aspas para lidar com vírgulas nos nomes
                dados.Add($"\"{user.Id}\",\"{user.Nome}\",\"{user.Email}\",\"{escolaNome}\",\"{turmaNome}\",\"{dataRegisto}\"");
            }

            await System.IO.File.WriteAllLinesAsync(filePath, dados);

            // 🔧 Guardar o caminho completo na coluna UrlDownload
            relatorio.UrlDownload = filePath;
            relatorio.Estado = EstadoRelatorio.Concluido;
            relatorio.DataConclusao = DateTime.UtcNow;

            await dbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao gerar relatório {Id}", relatorioId);

            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var relatorio = await dbContext.RelatoriosProgresso.FindAsync(relatorioId);
            if (relatorio != null)
            {
                relatorio.Estado = EstadoRelatorio.Erro;
                relatorio.DataConclusao = DateTime.UtcNow;
                relatorio.MensagemErro = ex.Message;
                await dbContext.SaveChangesAsync();
            }
        }
    }

    private RelatorioStatusDto MapToStatusDto(RelatorioProgresso r)
    {
        return new RelatorioStatusDto(
            r.Id,
            r.Titulo,
            "CSV", // Sempre retornar CSV pois é o único formato suportado agora
            r.Estado,
            r.DataSolicitacao,
            r.DataConclusao,
            r.Estado == EstadoRelatorio.Concluido ? $"/api/relatorios/{r.Id}/download" : null,
            r.MensagemErro // MensagemErro do relatório
        );
    }

    public async Task<RelatorioStatusDto?> GetStatusAsync(int id, CancellationToken ct = default)
    {
        var relatorio = await _context.RelatoriosProgresso
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == id, ct);

        return relatorio == null ? null : MapToStatusDto(relatorio);
    }

    public async Task<List<RelatorioListaDto>> ListarRelatoriosAsync(int utilizadorId, int? escolaId = null, CancellationToken ct = default)
    {
        var query = _context.RelatoriosProgresso
            .Where(r => r.UtilizadorId == utilizadorId)
            .AsNoTracking();

        if (escolaId.HasValue)
            query = query.Where(r => r.EscolaId == escolaId.Value);

        return await query
            .OrderByDescending(r => r.DataSolicitacao)
            .Select(r => new RelatorioListaDto(
                r.Id,
                r.Titulo,
                "CSV", // Sempre retornar CSV pois é o único formato suportado agora
                r.Estado,
                r.DataSolicitacao,
                r.DataConclusao
            ))
            .ToListAsync(ct);
    }

    public async Task<byte[]?> DownloadRelatorioAsync(int id, CancellationToken ct = default)
    {
        // 🔧 Obter o relatório pelo ID
        var relatorio = await _context.RelatoriosProgresso
            .FirstOrDefaultAsync(r => r.Id == id && r.Estado == EstadoRelatorio.Concluido, ct);

        if (relatorio == null || string.IsNullOrEmpty(relatorio.UrlDownload))
            return null;

        // Verificar se o ficheiro existe
        if (!System.IO.File.Exists(relatorio.UrlDownload))
        {
            // Atualizar estado para erro (se necessário)
            relatorio.Estado = EstadoRelatorio.Erro;
            relatorio.MensagemErro = "Arquivo do relatório não encontrado no servidor.";
            await _context.SaveChangesAsync(ct);
            return null;
        }

        // Ler e devolver os bytes do ficheiro
        return await System.IO.File.ReadAllBytesAsync(relatorio.UrlDownload, ct);
    }
}