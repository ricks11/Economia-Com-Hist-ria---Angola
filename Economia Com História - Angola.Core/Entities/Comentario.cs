using EconomiaComHistoria.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace EconomiaComHistoria.Core.Entities;

public class Comentario
{
    [Key] public int Id { get; set; }
    public string Texto { get; set; } = string.Empty;
    public EstadoComentario Estado { get; set; } = EstadoComentario.Pendente;
    public bool Editado { get; set; }
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
    public DateTime? EditadoEm { get; set; }
    public bool IsRespostaForum { get; set; }       // true = resposta num tópico de fórum
    public int? ComentarioPaiId { get; set; }       // para respostas aninhadas
    public bool IsSolucao { get; set; }             // marcado como solução

    public int AutorId { get; set; }
    public Utilizador Autor { get; set; } = null!;

    // Polimorfismo: comentário num conteúdo OU num tópico
    public int? ConteudoId { get; set; }
    public Conteudo? Conteudo { get; set; }
    public int? TopicoForumId { get; set; }
    public TopicoForum? TopicoForum { get; set; }

    // Auto-referência para aninhamento
    public Comentario? ComentarioPai { get; set; }
    public ICollection<Comentario> Respostas { get; set; } = new List<Comentario>();

    public ICollection<DenunciaConteudo> Denuncias { get; set; } = new List<DenunciaConteudo>();
    public ICollection<Reacao> Reacoes { get; set; } = new List<Reacao>();
}
