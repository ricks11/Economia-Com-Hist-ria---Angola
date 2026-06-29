using ECHA.Mobile.PageModels;

namespace ECHA.Mobile.Pages;

public partial class AchievementsPage : ContentPage
{
    public AchievementsPage(AchievementsPageModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
