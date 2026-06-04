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

    private void OnLanguageChanged(object sender, EventArgs e)
    {
        var picker = (Picker)sender;
        var traducao = (Models.TraducaoDto)picker.SelectedItem;
        if (traducao != null && BindingContext is ContentDetailPageModel viewModel && viewModel.Conteudo != null)
        {
            // Update UI content with translated text
            // Note: Simplification, assuming ViewModel handles the swap
        }
    }
}

