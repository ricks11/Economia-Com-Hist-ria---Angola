using CommunityToolkit.Mvvm.ComponentModel;

namespace ECHA.Mobile.PageModels;

public partial class LundaHeritageArquivosPageModel : ObservableObject
{
    public record ArquivoItem(string Title, string Description, string Icon);

    [ObservableProperty]
    private List<ArquivoItem> _arquivos = new()
    {
        new ArquivoItem("Plano de Fomento Agrário (1957)", "Relatório sobre investimentos.", "📄"),
        new ArquivoItem("Porto de Luanda (1972)", "Fotografia aérea histórica.", "📷")
    };

    public LundaHeritageArquivosPageModel()
    {
    }
}
