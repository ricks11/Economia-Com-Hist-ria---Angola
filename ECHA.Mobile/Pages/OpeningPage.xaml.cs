using ECHA.Mobile.PageModels;

namespace ECHA.Mobile.Pages;

public partial class OpeningPage : ContentPage
{
    public OpeningPage(OpeningPageModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
