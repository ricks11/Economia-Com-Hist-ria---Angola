using EconomiaComHistoria.Core.Entities;

namespace EconomiaComHistoria.Core.Interfaces;

public interface ISolicitacaoAcessoRepository : IRepository<SolicitacaoAcesso>
{
    Task<IEnumerable<SolicitacaoAcesso>> GetPendentesAsync();
    Task<IEnumerable<SolicitacaoAcesso>> GetByUtilizadorAsync(int utilizadorId);
    Task<IEnumerable<SolicitacaoAcesso>> GetByConteudoAsync(int conteudoId);
}