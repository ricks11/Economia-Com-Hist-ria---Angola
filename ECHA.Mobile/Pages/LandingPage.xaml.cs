using ECHA.Mobile.PageModels;

namespace ECHA.Mobile.Pages;

public partial class LandingPage : ContentPage
{
    public LandingPage(LandingPageModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
