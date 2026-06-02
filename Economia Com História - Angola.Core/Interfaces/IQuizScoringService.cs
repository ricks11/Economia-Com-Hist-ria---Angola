using EconomiaComHistoria.Core.Entities;

namespace EconomiaComHistoria.Core.Interfaces;

public interface IQuizScoringService
{
    /// <summary>
    /// Calculates the total score for a quiz attempt based on the answers provided.
    /// </summary>
    int CalcularPontuacao(TentativaQuiz tentativa, List<RespostaPergunta> respostas);
}
