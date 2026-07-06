using EconomiaComHistoria.Core.Entities;

namespace EconomiaComHistoria.Core.Interfaces;

public interface IPropostaQuizRepository : IRepository<PropostaQuiz>
{
    Task<IEnumerable<PropostaQuiz>> GetPendentesAsync();
    Task<IEnumerable<PropostaQuiz>> GetByUtilizadorAsync(int utilizadorId);
    Task<IEnumerable<PropostaQuiz>> GetByTopicoForumAsync(int topicoId);
}