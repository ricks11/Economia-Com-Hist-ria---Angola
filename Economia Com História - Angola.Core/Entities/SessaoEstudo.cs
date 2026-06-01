namespace EconomiaComHistoria.Core.Entities;

public class SessaoEstudo
{
    public int Id { get; set; }
    public int UtilizadorId { get; set; }
    public Utilizador? Utilizador { get; set; }
    public DateTime Inicio { get; set; }
    public DateTime? Fim { get; set; }
    public int DuracaoMinutos { get; set; }
}
