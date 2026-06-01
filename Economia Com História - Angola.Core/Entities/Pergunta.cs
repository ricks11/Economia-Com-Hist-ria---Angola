namespace EconomiaComHistoria.Core.Entities;

public class Pergunta
{
    public int Id { get; set; }
    public string Texto { get; set; } = string.Empty;
    public string? OpcaoA { get; set; }
    public string? OpcaoB { get; set; }
    public string? OpcaoC { get; set; }
    public string? OpcaoD { get; set; }
    public string? RespostaCorreta { get; set; }
    public int QuizId { get; set; }
    public Quiz? Quiz { get; set; }
    public ICollection<RespostaPergunta> Respostas { get; set; } = new List<RespostaPergunta>();
}
