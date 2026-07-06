using EconomiaComHistoria.Core.Entities;
using EconomiaComHistoria.Core.Interfaces;
using EconomiaComHistoria.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EconomiaComHistoria.Infrastructure.Repositories;

public class SolicitacaoAcessoRepository : BaseRepository<SolicitacaoAcesso>, ISolicitacaoAcessoRepository
{
    public SolicitacaoAcessoRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<SolicitacaoAcesso>> GetPendentesAsync()
        => await _dbSet
            .Where(s => s.Status == "Pendente")
            .Include(s => s.Utilizador)
            .Include(s => s.Conteudo)
            .OrderBy(s => s.DataSolicitacao)
            .ToListAsync();

    public async Task<IEnumerable<SolicitacaoAcesso>> GetByUtilizadorAsync(int utilizadorId)
        => await _dbSet
            .Where(s => s.UtilizadorId == utilizadorId)
            .Include(s => s.Conteudo)
            .ToListAsync();

    public async Task<IEnumerable<SolicitacaoAcesso>> GetByConteudoAsync(int conteudoId)
        => await _dbSet
            .Where(s => s.ConteudoId == conteudoId)
            .Include(s => s.Utilizador)
            .ToListAsync();
}