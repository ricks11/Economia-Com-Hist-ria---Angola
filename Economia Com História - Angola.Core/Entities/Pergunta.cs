using EconomiaComHistoria.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace EconomiaComHistoria.Core.Entities;

public class Pergunta
{
    [Key] public int Id { get; set; }
    public string Enunciado { get; set; } = string.Empty;
    public string Explicacao { get; set; } = string.Empty;
    public int Pontos { get; set; } = 100;
    public string Tema { get; set; } = string.Empty;
    public NivelDificuldade Dificuldade { get; set; }
    public int NumVezesErrada { get; set; }
    public EstadoPergunta Estado { get; set; } = EstadoPergunta.Ativa;
    public DateTime CriadaEm { get; set; } = DateTime.UtcNow;

    public int QuizId { get; set; }
    public Quiz Quiz { get; set; } = null!;

    // Criador/validador (Editor ou Professor)
    public int? AutorId { get; set; }
    public Utilizador? Autor { get; set; }

    public ICollection<RespostaPergunta> Respostas { get; set; } = new List<RespostaPergunta>();
    public ICollection<OpcaoResposta> Opcoes { get; set; } = new List<OpcaoResposta>();
}
