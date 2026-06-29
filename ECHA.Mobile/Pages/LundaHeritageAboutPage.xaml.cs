using ECHA.Mobile.PageModels;

namespace ECHA.Mobile.Pages;

public partial class LundaHeritageAboutPage : ContentPage
{
    public LundaHeritageAboutPage(LundaHeritageAboutPageModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
