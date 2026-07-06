using System.ComponentModel.DataAnnotations;

namespace EconomiaComHistoria.Core.Entities;

public class AuditoriaLog
{
    [Key] public int Id { get; set; }
    public string Acao { get; set; } = string.Empty;         // ex: "publicar_conteudo"
    public string Recurso { get; set; } = string.Empty;      // ex: "Conteudo"
    public int IdRecurso { get; set; }
    public string? DadosAntes { get; set; }     // JSON opcional
    public string? DadosDepois { get; set; }    // JSON opcional
    public string Ip { get; set; } = string.Empty;
    public string? UserAgent { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string? SessaoHash { get; set; }

    public int UtilizadorId { get; set; }
    public Utilizador Utilizador { get; set; } = null!;
    public string? Sessao { get; set; }
}
