using EconomiaComHistoria.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace EconomiaComHistoria.Core.Entities;

public class Conteudo
{
    [Key] public int Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public TipoConteudo Tipo { get; set; }
    public string Tema { get; set; } = string.Empty;
    public NivelDificuldade Nivel { get; set; }
    public string Regiao { get; set; } = string.Empty;
    public int DuracaoMinutos { get; set; }
    public string? Resumo { get; set; }
    public string? ThumbnailUrl { get; set; }
    public string? CorpoTexto { get; set; }
    public string? VideoUrl { get; set; }
    public string? AudioUrl { get; set; }
    public EstadoConteudo Estado { get; set; } = EstadoConteudo.Rascunho;
    public int Visualizacoes { get; set; }
    public Visibilidade Visibilidade { get; set; } = Visibilidade.Publico;
    public bool ComentariosDesativados { get; set; } = false;
    public bool DisponiveOffline { get; set; }
    public bool IsJindungo { get; set; }
    public string? ReferenciaFactual { get; set; }    // obrigatório se IsJindungo = true
    public string? AlertaOpiniao { get; set; }        // texto do aviso Jindungo
    public string? AutorOpiniao { get; set; }
    public DateTime? DataPublicacao { get; set; }
    public DateTime? DataAgendada { get; set; }
    public int VersaoAtual { get; set; } = 1;
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
    public DateTime? AtualizadoEm { get; set; }

    // Editor que criou/publicou
    public int? EditorId { get; set; }
    public Utilizador? Editor { get; set; }

    public ICollection<VersaoConteudo> Versoes { get; set; } = new List<VersaoConteudo>();
    public ICollection<Comentario> Comentarios { get; set; } = new List<Comentario>();
    public ICollection<VisualizacaoConteudo> Visualizacoes2 { get; set; } = new List<VisualizacaoConteudo>();
    public ICollection<ConteudoFavorito> Favoritos { get; set; } = new List<ConteudoFavorito>();
    public ICollection<ConteudoTraducao> Traducoes { get; set; } = new List<ConteudoTraducao>();
    public ICollection<SessaoEstudo> Sessoes { get; set; } = new List<SessaoEstudo>();
    public Quiz? Quiz { get; set; }
}
