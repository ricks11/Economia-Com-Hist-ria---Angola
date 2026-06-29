using ECHA.Mobile.PageModels;

namespace ECHA.Mobile.Pages;

public partial class JindungoAccessPage : ContentPage
{
    public JindungoAccessPage(JindungoAccessPageModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
