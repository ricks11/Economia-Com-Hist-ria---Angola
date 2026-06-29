using ECHA.Mobile.PageModels;

namespace ECHA.Mobile.Pages;

public partial class TurmaRankingPage : ContentPage
{
    public TurmaRankingPage(TurmaRankingPageModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
