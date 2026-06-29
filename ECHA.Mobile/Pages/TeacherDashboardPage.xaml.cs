using ECHA.Mobile.PageModels;

namespace ECHA.Mobile.Pages;

public partial class TeacherDashboardPage : ContentPage
{
    public TeacherDashboardPage(TeacherDashboardPageModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
