using EconomiaComHistoria.Core.Enums;

namespace EconomiaComHistoria.Core.Entities;

public class TopicoForum
{
    public int Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Conteudo { get; set; } = string.Empty;
    public int AutorId { get; set; }
    public Utilizador? Autor { get; set; }
    public int CategoriaId { get; set; }
    public CategoriaForum? Categoria { get; set; }
    public EstadoTopico EstadoTopico { get; set; } = EstadoTopico.Pendente;
    public DateTime DataCriacao { get; set; }
    public int TotalDenuncias { get; set; }
    public string? MotivoRejeicao { get; set; }
    public ICollection<RespostaForum> Respostas { get; set; } = new List<RespostaForum>();
    public ICollection<Reacao> Reacoes { get; set; } = new List<Reacao>();
    public ICollection<DenunciaConteudo> Denuncias { get; set; } = new List<DenunciaConteudo>();
}
