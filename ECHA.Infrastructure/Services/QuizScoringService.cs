using EconomiaComHistoria.Core.Entities;
using EconomiaComHistoria.Core.Interfaces;

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
        int basePoints = 100 * (tentativa.Quiz.NivelDificuldade + 1);

        foreach (var resposta in respostas)
        {
            if (resposta.IsCorrecta && resposta.Pergunta != null)
            {
                int tempoLimiteMs = resposta.Pergunta.TempoLimiteSegundos * 1000;
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
