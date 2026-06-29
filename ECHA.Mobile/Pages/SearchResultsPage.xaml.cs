using ECHA.Mobile.PageModels;

namespace ECHA.Mobile.Pages;

public partial class SearchResultsPage : ContentPage
{
    public SearchResultsPage(SearchResultsPageModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
