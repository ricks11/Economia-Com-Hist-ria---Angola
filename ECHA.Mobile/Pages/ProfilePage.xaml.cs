using ECHA.Mobile.PageModels;

namespace ECHA.Mobile.Pages;

public partial class ProfilePage : ContentPage
{
    private readonly ProfilePageModel _viewModel;

    public ProfilePage(ProfilePageModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadAsync();
    }
}
