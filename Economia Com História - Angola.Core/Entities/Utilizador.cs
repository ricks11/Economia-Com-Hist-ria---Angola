using System.ComponentModel.DataAnnotations;
using EconomiaComHistoria.Core.Enums;

namespace EconomiaComHistoria.Core.Entities;

public class Utilizador
{
    [Key] public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Telemovel { get; set; }
    public string PasswordHash { get; set; } = string.Empty;
    public TipoUtilizador Tipo { get; set; } = TipoUtilizador.Visitante;
    public DateTime DataRegisto { get; set; } = DateTime.UtcNow;
    public DateTime? UltimoAcesso { get; set; }
    public int PontosTotais { get; set; }
    public int StreakAtual { get; set; }
    public int StreakRecorde { get; set; }
    public int TempoEstudoMinutos { get; set; }
    public string? AvatarConfig { get; set; }
    public string? Provincia { get; set; }
    public string? Municipio { get; set; }
    public bool Ativo { get; set; } = true;
    public bool TwoFactorAtivo { get; set; }
    public string? TwoFactorSegredo { get; set; }
    public string? IpUltimoLogin { get; set; }
    public int NumeroPublicacoes { get; set; }     // controlo moderação prévia (<5)
    public DateTime? SuspensoAte { get; set; }      // null = não suspenso

    // Indica se o utilizador está atualmente sob o efeito de qualquer suspensão
    public bool Suspenso => SuspensaoPermanente || (SuspensoAte.HasValue && SuspensoAte.Value > DateTime.UtcNow);

    // Consideramos um banimento permanente se a data for muito longa no futuro (ex: 100 anos como definiste no modal)
    // ou se preferires, define uma regra com base no ano configurado (ex: ano > 2100)
    public bool SuspensaoPermanente => SuspensoAte.HasValue && SuspensoAte.Value.Year >= DateTime.UtcNow.Year + 90;

    // Navigation properties
    public int? EscolaId { get; set; }
    public Escola? Escola { get; set; }
    public int? TurmaId { get; set; }
    public Turma? Turma { get; set; }

    public ICollection<TentativaQuiz> TentativasQuiz { get; set; } = new List<TentativaQuiz>();
    public ICollection<TopicoForum> TopicosForum { get; set; } = new List<TopicoForum>();
    public ICollection<RespostaForum> RespostaForums { get; set; } = new List<RespostaForum>();
    public ICollection<Comentario> Comentarios { get; set; } = new List<Comentario>();
    public ICollection<BadgeConquistado> BadgesConquistados { get; set; } = new List<BadgeConquistado>();
    public ICollection<SessaoEstudo> SessoesEstudo { get; set; } = new List<SessaoEstudo>();
    public ICollection<EventoGamificacao> EventosGamificacao { get; set; } = new List<EventoGamificacao>();
    public ICollection<VisualizacaoConteudo> Visualizacoes { get; set; } = new List<VisualizacaoConteudo>();
    public ICollection<ConteudoFavorito> Favoritos { get; set; } = new List<ConteudoFavorito>();
    public PlanoEstudo? PlanoEstudo { get; set; }
    public ICollection<ModuloProgresso> ProgressoModulos { get; set; } = new List<ModuloProgresso>();
}
