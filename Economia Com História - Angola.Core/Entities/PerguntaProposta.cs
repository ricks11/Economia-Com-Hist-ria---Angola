using System.ComponentModel.DataAnnotations;

namespace EconomiaComHistoria.Core.Entities;

public class PerguntaProposta
{
    [Key] public int Id { get; set; }
    public int PropostaQuizId { get; set; }
    public PropostaQuiz PropostaQuiz { get; set; } = null!;
    public string Enunciado { get; set; } = string.Empty;
    public string[] Opcoes { get; set; } = Array.Empty<string>();
    public int IndiceCorreto { get; set; }
    public string Explicacao { get; set; } = string.Empty;
    public int Pontos { get; set; } = 100;
}