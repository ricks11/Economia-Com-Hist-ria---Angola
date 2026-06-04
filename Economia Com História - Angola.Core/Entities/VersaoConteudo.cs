using System.ComponentModel.DataAnnotations;

namespace EconomiaComHistoria.Core.Entities;

public class VersaoConteudo
{
    [Key] public int Id { get; set; }
    public int NumeroVersao { get; set; }
    /// <summary>Snapshot JSON completo do conteúdo nesta versão</summary>
    public string SnapshotJson { get; set; } = "{}";
    public string? Motivo { get; set; }
    public DateTime DataGuardado { get; set; } = DateTime.UtcNow;

    public int ConteudoId { get; set; }
    public Conteudo Conteudo { get; set; } = null!;
    public int AutorId { get; set; }
    public Utilizador Autor { get; set; } = null!;
}
