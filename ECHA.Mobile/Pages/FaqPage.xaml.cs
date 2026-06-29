using ECHA.Mobile.PageModels;

namespace ECHA.Mobile.Pages;

public partial class FaqPage : ContentPage
{
    public FaqPage(FaqPageModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
