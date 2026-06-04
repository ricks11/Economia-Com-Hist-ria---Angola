using EconomiaComHistoria.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace EconomiaComHistoria.Core.Entities;

public class Moderacao
{
    [Key] public int Id { get; set; }
    public TipoAlvoModeracao TipoAlvo { get; set; }
    public int IdAlvo { get; set; }
    public EstadoModeracao Estado { get; set; } = EstadoModeracao.EmFila;
    public PrioridadeModeracao Prioridade { get; set; } = PrioridadeModeracao.Normal;
    public DateTime DataAbertura { get; set; } = DateTime.UtcNow;
    public DateTime? DataResolucao { get; set; }
    public DateTime? PrazoRevisao { get; set; }    // ex: suspenso automático → 48h

    public int? ModeradorId { get; set; }
    public Utilizador? Moderador { get; set; }

    public ICollection<DecisaoModeracao> Decisoes { get; set; } = new List<DecisaoModeracao>();
    public ICollection<DenunciaConteudo> Denuncias { get; set; } = new List<DenunciaConteudo>();
}
