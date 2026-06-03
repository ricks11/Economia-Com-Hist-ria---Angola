using ECHA.Mobile.PageModels;

namespace ECHA.Mobile.Pages;

public partial class ContentDetailPage : ContentPage
{
    public ContentDetailPage(ContentDetailPageModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is ContentDetailPageModel viewModel)
        {
            await viewModel.TrackViewCommand.ExecuteAsync(null);
        }
    }
}
