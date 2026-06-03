namespace EconomiaComHistoria.Core.Entities;

public class ConteudoJindungo : Conteudo
{
    public string ReferenciaFactual { get; set; } = string.Empty;
    public string? Origem { get; set; }
    public DateTime? DataHistorica { get; set; }
}
