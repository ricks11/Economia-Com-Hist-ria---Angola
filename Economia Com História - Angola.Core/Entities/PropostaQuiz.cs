using System.ComponentModel.DataAnnotations;

namespace EconomiaComHistoria.Core.Entities;

public class PropostaQuiz
{
    [Key] public int Id { get; set; }
    public int UtilizadorId { get; set; }
    public Utilizador Utilizador { get; set; } = null!;
    public int? TopicoForumId { get; set; }
    public TopicoForum? TopicoForum { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Tema { get; set; } = string.Empty;
    public string Nivel { get; set; } = string.Empty;
    public string Status { get; set; } = "Pendente";
    public DateTime DataProposta { get; set; } = DateTime.UtcNow;
    public DateTime? DataDecisao { get; set; }
    public int? EditorId { get; set; }
    public List<PerguntaProposta> Perguntas { get; set; } = new();
}