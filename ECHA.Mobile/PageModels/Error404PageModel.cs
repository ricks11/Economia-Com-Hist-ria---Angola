using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ECHA.Mobile.PageModels;

public partial class Error404PageModel : ObservableObject
{
    public Error404PageModel()
    {
    }

    [RelayCommand]
    private async Task GoHome()
    {
        if (Shell.Current != null)
        {
            await Shell.Current.GoToAsync("//MainPage");
        }
    }
}
