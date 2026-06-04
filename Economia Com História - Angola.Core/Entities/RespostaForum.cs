using EconomiaComHistoria.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace EconomiaComHistoria.Core.Entities;

public class RespostaForum
{
    [Key] public int Id { get; set; }
    public int TopicoId { get; set; }
    public TopicoForum? Topico { get; set; }
    public int AutorId { get; set; }
    public Utilizador? Autor { get; set; }
    public string Conteudo { get; set; } = string.Empty;
    public bool IsSolucao { get; set; }
    public bool ComentariosDesativados { get; set; }
    public int? RespostaPaiId { get; set; }
    public RespostaForum? RespostaPai { get; set; }
    public EstadoComentario EstadoResposta { get; set; } = EstadoComentario.Pendente;
    public DateTime DataCriacao { get; set; }
    public DateTime? DataEdicao { get; set; }
    public ICollection<RespostaForum> RespostasFilhas { get; set; } = new List<RespostaForum>();
    public ICollection<Reacao> Reacoes { get; set; } = new List<Reacao>();
    public ICollection<DenunciaConteudo> Denuncias { get; set; } = new List<DenunciaConteudo>();
}
