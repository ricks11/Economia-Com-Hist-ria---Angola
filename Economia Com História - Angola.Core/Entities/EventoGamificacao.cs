using EconomiaComHistoria.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace EconomiaComHistoria.Core.Entities;

public class EventoGamificacao
{
    [Key] public int Id { get; set; }
    public TipoEventoGamificacao Tipo { get; set; }
    public int PontosAtribuidos { get; set; }
    public float Multiplicador { get; set; } = 1f;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public int UtilizadorId { get; set; }
    public Utilizador Utilizador { get; set; } = null!;
}
