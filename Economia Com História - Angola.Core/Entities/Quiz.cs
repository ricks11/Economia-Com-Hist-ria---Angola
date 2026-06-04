using EconomiaComHistoria.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace EconomiaComHistoria.Core.Entities;

public class Quiz
{
    [Key] public int Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Tema { get; set; } = string.Empty;
    public NivelDificuldade Nivel { get; set; }
    public int TotalPerguntas { get; set; }
    public int TempoLimiteSeg { get; set; } = 30;
    public bool Ativo { get; set; } = true;

    public int? ConteudoId { get; set; }
    public Conteudo? Conteudo { get; set; }

    public ICollection<Pergunta> Perguntas { get; set; } = new List<Pergunta>();
    public ICollection<TentativaQuiz> Tentativas { get; set; } = new List<TentativaQuiz>();
}
