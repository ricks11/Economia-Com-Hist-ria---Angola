using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ECHA.Mobile.PageModels;

public partial class LundaHeritageAboutPageModel : ObservableObject
{
    public LundaHeritageAboutPageModel()
    {
    }

    [RelayCommand]
    private async Task NavigateToArquivosAsync()
    {
        await Shell.Current.GoToAsync("LundaHeritageArquivosPage");
    }
}
