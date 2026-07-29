using ECHA.Mobile.Data;
using CommunityToolkit.Maui.Views;
using ECHA.Mobile.Pages.Popups;
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

            if (viewModel.Conteudo?.IsJindungo == true)
            {
                await this.ShowPopupAsync(new JindungoDisclaimerPopup());
            }
        }
    }

    private async void OnBackButtonClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}
