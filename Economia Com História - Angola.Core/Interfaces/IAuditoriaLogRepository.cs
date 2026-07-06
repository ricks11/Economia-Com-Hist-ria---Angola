using EconomiaComHistoria.Core.Entities;

namespace EconomiaComHistoria.Core.Interfaces;

public interface IAuditoriaLogRepository : IRepository<AuditoriaLog>
{
    Task<IEnumerable<AuditoriaLog>> GetByUtilizadorAsync(int utilizadorId);
    Task<IEnumerable<AuditoriaLog>> GetByAcaoAsync(string acao);
    Task<IEnumerable<AuditoriaLog>> GetByPeriodoAsync(DateTime inicio, DateTime fim);
}