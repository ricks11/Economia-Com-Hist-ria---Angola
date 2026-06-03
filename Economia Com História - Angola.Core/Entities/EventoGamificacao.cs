using EconomiaComHistoria.Core.Enums;

namespace EconomiaComHistoria.Core.Entities;

public class EventoGamificacao
{
    public int Id { get; set; }
    public TipoEventoGamificacao Tipo { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public int PontosGanhos { get; set; }
    public DateTime DataEvento { get; set; }
    public int UtilizadorId { get; set; }
    public Utilizador? Utilizador { get; set; }
}
