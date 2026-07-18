using EconomiaComHistoria.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace EconomiaComHistoria.Core.Entities;

public class TopicoForum
{
    [Key] public int Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public int CategoriaId { get; set; }
    public CategoriaForum Categoria { get; set; } = null!;
    public string? TagsSerialized { get; set; }     // JSON array de strings
    public EstadoTopicoForum Estado { get; set; } = EstadoTopicoForum.Ativo;
    public bool Fixado { get; set; }
    public bool EspecialistaRespondeu { get; set; }
    public int Visualizacoes { get; set; }
    public bool ComentariosDesativados { get; set; }
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
    public DateTime? SuspensoAte { get; set; }

    public int AutorId { get; set; }
    public Utilizador Autor { get; set; } = null!;

    // Novas propriedades para visibilidade
    public Visibilidade Visibilidade { get; set; } = Visibilidade.Publico;
    public int? EscolaId { get; set; }
    public Escola? Escola { get; set; }
    public int? TurmaId { get; set; }
    public Turma? Turma { get; set; }

    public ICollection<RespostaForum> Respostas { get; set; } = new List<RespostaForum>();
    public ICollection<DenunciaConteudo> Denuncias { get; set; } = new List<DenunciaConteudo>();
    public ICollection<Reacao> Reacoes { get; set; } = new List<Reacao>();
}
