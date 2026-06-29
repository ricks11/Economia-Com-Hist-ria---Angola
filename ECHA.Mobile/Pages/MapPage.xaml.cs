using ECHA.Mobile.PageModels;

namespace ECHA.Mobile.Pages;

public partial class MapPage : ContentPage
{
    public MapPage(MapPageModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
