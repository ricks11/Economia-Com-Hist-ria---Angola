using EconomiaComHistoria.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace EconomiaComHistoria.Core.Entities;

public class EventoGamificacao
{
    [Key] public int Id { get; set; }
    public TipoEventoGamificacao Tipo { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public int PontosGanhos { get; set; }
    public DateTime DataEvento { get; set; } = DateTime.UtcNow;

    public int UtilizadorId { get; set; }
    public Utilizador Utilizador { get; set; } = null!;
}
