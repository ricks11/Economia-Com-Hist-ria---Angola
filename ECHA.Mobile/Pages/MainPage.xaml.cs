using ECHA.Mobile.PageModels;

namespace ECHA.Mobile.Pages;

public partial class MainPage : ContentPage
{
    private readonly MainPageModel _model;

    public MainPage(MainPageModel model)
    {
        InitializeComponent();
        BindingContext = _model = model;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _model.AppearingCommand.ExecuteAsync(null);
    }
}
