using EconomiaComHistoria.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace EconomiaComHistoria.Core.Entities;

public class DenunciaConteudo
{
    [Key] public int Id { get; set; }
    public MotivoDenuncia Motivo { get; set; }
    public string? Descricao { get; set; }
    public EstadoDenuncia Estado { get; set; } = EstadoDenuncia.Pendente;
    public DateTime DataDenuncia { get; set; } = DateTime.UtcNow;
    public TipoAlvoModeracao TipoAlvo { get; set; }
    public int IdAlvo { get; set; }             // id do tópico ou comentário

    public int UtilizadorId { get; set; }
    public Utilizador Utilizador { get; set; } = null!;

    public int? TopicoForumId { get; set; }
    public TopicoForum? TopicoForum { get; set; }
    public int? RespostaForumId { get; set; }        // era ComentarioId
    public RespostaForum? RespostaForum { get; set; } // era Comentario

    // Moderação gerada a partir desta denúncia
    public int? ModeracaoId { get; set; }
    public Moderacao? Moderacao { get; set; }
}
