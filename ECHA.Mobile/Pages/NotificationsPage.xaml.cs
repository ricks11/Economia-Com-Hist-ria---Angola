using ECHA.Mobile.PageModels;

namespace ECHA.Mobile.Pages;

public partial class NotificationsPage : ContentPage
{
    public NotificationsPage(NotificationsPageModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
