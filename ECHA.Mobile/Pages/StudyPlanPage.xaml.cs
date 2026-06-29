using ECHA.Mobile.PageModels;

namespace ECHA.Mobile.Pages;

public partial class StudyPlanPage : ContentPage
{
    public StudyPlanPage(StudyPlanPageModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
