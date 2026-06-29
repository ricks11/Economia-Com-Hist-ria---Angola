using ECHA.Mobile.PageModels;

namespace ECHA.Mobile.Pages;

public partial class Error404Page : ContentPage
{
    public Error404Page(Error404PageModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
