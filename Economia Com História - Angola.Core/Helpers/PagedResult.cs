namespace EconomiaComHistoria.Core.Helpers;

public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int Pagina { get; set; }
    public int TamanhoTotal { get; set; }
    public int TotalPaginas { get; set; }

    public static PagedResult<T> Create(IEnumerable<T> items, int totalCount, int pagina, int tamanho)
    {
        var totalPaginas = (int)Math.Ceiling((double)totalCount / tamanho);

        return new PagedResult<T>
        {
            Items = items.ToList(),
            Pagina = pagina,
            TamanhoTotal = totalCount,
            TotalPaginas = totalPaginas
        };
    }
}
