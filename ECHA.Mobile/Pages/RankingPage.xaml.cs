using ECHA.Mobile.PageModels;

namespace ECHA.Mobile.Pages;

public partial class RankingPage : ContentPage
{
    public RankingPage(RankingPageModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
