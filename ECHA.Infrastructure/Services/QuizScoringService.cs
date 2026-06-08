using EconomiaComHistoria.Core.Interfaces;
using EconomiaComHistoria.Core.Enums;
using EconomiaComHistoria.Core.Entities;

namespace EconomiaComHistoria.Infrastructure.Services;

public class QuizScoringService : IQuizScoringService
{
    public int CalcularPontuacao(TentativaQuiz tentativa, List<RespostaPergunta> respostas)
    {
        if (tentativa.Quiz == null)
        {
            throw new ArgumentException("Quiz must be loaded to calculate score.");
        }

        double totalScore = 0;
        int basePoints = 100 * ((int)tentativa.Quiz.Nivel + 1);

        foreach (var resposta in respostas)
        {
            if (resposta.IsCorrecta)
            {
                int tempoLimiteMs = tentativa.Quiz.TempoLimiteSeg * 1000;
                double bonusVelocidade = 0;

                if (tempoLimiteMs > 0)
                {
                    bonusVelocidade = Math.Max(0, ((double)(tempoLimiteMs - resposta.TempoRespostaMs) / tempoLimiteMs) * 50);
                }

                totalScore += basePoints + bonusVelocidade;
            }
        }

        return (int)Math.Round(totalScore);
    }
}
