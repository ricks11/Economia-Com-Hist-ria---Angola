using System.ComponentModel.DataAnnotations;

namespace EconomiaComHistoria.Core.Entities;

public class EntradaRanking
{
    [Key] public int Id { get; set; }
    public int Posicao { get; set; }
    public int Pontos { get; set; }
    public int QuizzesCompletados { get; set; }

    public int RankingId { get; set; }
    public Ranking Ranking { get; set; } = null!;
    public int UtilizadorId { get; set; }
    public Utilizador Utilizador { get; set; } = null!;
    public int? EscolaId { get; set; }
    public Escola? Escola { get; set; }
}
