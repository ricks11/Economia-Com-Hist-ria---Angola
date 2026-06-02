using EconomiaComHistoria.Core.Enums;

namespace EconomiaComHistoria.Core.Entities;

public class Reacao
{
    public int Id { get; set; }
    public int UtilizadorId { get; set; }
    public Utilizador? Utilizador { get; set; }
    public int? TopicoId { get; set; }
    public TopicoForum? Topico { get; set; }
    public int? RespostaId { get; set; }
    public RespostaForum? Resposta { get; set; }
    public TipoReacao TipoReacao { get; set; }
    public int? ConteudoId { get; set; }
    public Conteudo? Conteudo { get; set; }
    public int? ComentarioId { get; set; }
    public Comentario? Comentario { get; set; }
}
