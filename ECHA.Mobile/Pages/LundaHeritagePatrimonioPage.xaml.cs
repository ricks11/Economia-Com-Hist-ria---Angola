using ECHA.Mobile.PageModels;

namespace ECHA.Mobile.Pages;

public partial class LundaHeritagePatrimonioPage : ContentPage
{
    public LundaHeritagePatrimonioPage(LundaHeritagePatrimonioPageModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
