using ECHA.Mobile.PageModels;

namespace ECHA.Mobile.Pages;

public partial class QuizPage : ContentPage
{
    public QuizPage(QuizPageModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
