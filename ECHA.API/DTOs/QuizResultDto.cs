namespace EconomiaComHistoria.API.DTOs;

public class QuizResultDto
{
    public int QuizId { get; set; }
    public int UtilizadorId { get; set; }
    public int Pontuacao { get; set; }
    public TimeSpan TempoTotal { get; set; }
}
