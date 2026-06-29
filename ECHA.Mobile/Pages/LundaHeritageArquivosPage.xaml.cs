using ECHA.Mobile.PageModels;

namespace ECHA.Mobile.Pages;

public partial class LundaHeritageArquivosPage : ContentPage
{
    public LundaHeritageArquivosPage(LundaHeritageArquivosPageModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
