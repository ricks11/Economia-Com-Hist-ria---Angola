using ECHA.Mobile.PageModels;

namespace ECHA.Mobile.Pages;

public partial class LoginPage : ContentPage
{
    public LoginPage(LoginPageModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
