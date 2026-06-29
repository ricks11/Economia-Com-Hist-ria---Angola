using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ECHA.Mobile.PageModels;

public partial class OpeningPageModel : ObservableObject
{
    public OpeningPageModel()
    {
    }

    [RelayCommand]
    private async Task EnterPortal()
    {
        if (Shell.Current != null)
        {
            await Shell.Current.GoToAsync("//LandingPage");
        }
    }
}
