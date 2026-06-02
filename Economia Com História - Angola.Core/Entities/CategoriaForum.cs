namespace EconomiaComHistoria.Core.Entities;

public class CategoriaForum
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public string? Icone { get; set; }
    public ICollection<TopicoForum> Topicos { get; set; } = new List<TopicoForum>();
}
