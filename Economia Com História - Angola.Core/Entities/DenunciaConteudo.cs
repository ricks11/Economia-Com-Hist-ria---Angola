namespace EconomiaComHistoria.Core.Entities;

public class DenunciaConteudo
{
    public int Id { get; set; }
    public int DenuncianteId { get; set; }
    public Utilizador? Denunciante { get; set; }
    public int? TopicoId { get; set; }
    public TopicoForum? Topico { get; set; }
    public int? RespostaId { get; set; }
    public RespostaForum? Resposta { get; set; }
    public string Motivo { get; set; } = string.Empty;
    public DateTime DataDenuncia { get; set; }
    public DateTime DataCriacao { get; set; }
    public int? ConteudoId { get; set; }
    public Conteudo? Conteudo { get; set; }
    public int? ComentarioId { get; set; }
    public Comentario? Comentario { get; set; }
}
