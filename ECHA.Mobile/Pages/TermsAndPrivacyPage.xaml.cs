using ECHA.Mobile.PageModels;

namespace ECHA.Mobile.Pages;

public partial class TermsAndPrivacyPage : ContentPage
{
    public TermsAndPrivacyPage(TermsAndPrivacyPageModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
