using EconomiaComHistoria.Core.Enums;

namespace EconomiaComHistoria.Core.Entities;

public class Conteudo
{
    public int Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string? Resumo { get; set; }
    public string? Texto { get; set; }
    public DateTime DataPublicacao { get; set; }
    public int AutorId { get; set; }
    public Utilizador? Autor { get; set; }
    public string? Tema { get; set; }
    public string? Nivel { get; set; }
    public string? Regiao { get; set; }
    public string? Tipo { get; set; }
    public EstadoConteudo Estado { get; set; } = EstadoConteudo.Ativo;
    public string? ImagemCapa { get; set; }
    public int Visualizacoes { get; set; } = 0;
    public ICollection<Quiz> Quizzes { get; set; } = new List<Quiz>();
    public ICollection<VisualizacaoConteudo> Visualizacoes_Rastreamento { get; set; } = new List<VisualizacaoConteudo>();
    public ICollection<ConteudoFavorito> Favoritos { get; set; } = new List<ConteudoFavorito>();
}
