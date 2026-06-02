namespace EconomiaComHistoria.Core.Entities;

public class OpcaoResposta
{
    public int Id { get; set; }
    public int PerguntaId { get; set; }
    public Pergunta? Pergunta { get; set; }
    public string Texto { get; set; } = string.Empty;
    public bool IsCorrecta { get; set; }
    public string? Explicacao { get; set; }
}
