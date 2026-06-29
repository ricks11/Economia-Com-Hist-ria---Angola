using CommunityToolkit.Mvvm.ComponentModel;

namespace ECHA.Mobile.PageModels;

public partial class FaqPageModel : ObservableObject
{
    public record FaqItem(string Question, string Answer);

    [ObservableProperty]
    private List<FaqItem> _faqItems = new()
    {
        new FaqItem("Como citar documentos?", "Utilize a norma APA ou NP 405 disponível em cada documento."),
        new FaqItem("Funciona offline?", "Apenas se descarregar os ficheiros previamente."),
        new FaqItem("Como aceder ao Jindungo?", "Na página de perfil, encontre a secção 'Acesso ao Jindungo' e siga as instruções."),
        new FaqItem("Como reportar um problema?", "Utilize a página de contacto ou o botão de denúncia em discussões do fórum.")
    };

    [ObservableProperty]
    private string _searchText = string.Empty;

    public FaqPageModel()
    {
    }
}
