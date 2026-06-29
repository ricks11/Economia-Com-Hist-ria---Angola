using ECHA.Mobile.PageModels;

namespace ECHA.Mobile.Pages;

public partial class ContactPage : ContentPage
{
    public ContactPage(ContactPageModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
