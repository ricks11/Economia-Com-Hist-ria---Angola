using EconomiaComHistoria.Core.Enums;

namespace EconomiaComHistoria.Core.Entities;

public class Utilizador
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Telemovel { get; set; }
    public string PasswordHash { get; set; } = string.Empty;
    public TipoUtilizador Tipo { get; set; }
    public DateTime DataRegisto { get; set; }
    public int PontosTotais { get; set; }
    public int StreakAtual { get; set; }
    public int? EscolaId { get; set; }
    public Escola? Escola { get; set; }
    public ICollection<TentativaQuiz> TentativasQuiz { get; set; } = new List<TentativaQuiz>();
    public ICollection<TopicoForum> Topicos { get; set; } = new List<TopicoForum>();
    public ICollection<Conteudo> Conteudos { get; set; } = new List<Conteudo>();
    public ICollection<Comentario> Comentarios { get; set; } = new List<Comentario>();
    public ICollection<Reacao> Reacoes { get; set; } = new List<Reacao>();
    public ICollection<BadgeConquistado> BadgesConquistados { get; set; } = new List<BadgeConquistado>();
    public ICollection<EventoGamificacao> EventosGamificacao { get; set; } = new List<EventoGamificacao>();
    public ICollection<SessaoEstudo> SessoesEstudo { get; set; } = new List<SessaoEstudo>();
    public ICollection<PlanoEstudo> PlanosEstudo { get; set; } = new List<PlanoEstudo>();
    public ICollection<RelatorioProgresso> RelatoriosProgresso { get; set; } = new List<RelatorioProgresso>();
}
