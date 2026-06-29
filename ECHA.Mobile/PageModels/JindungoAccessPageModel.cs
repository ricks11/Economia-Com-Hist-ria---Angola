using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ECHA.Mobile.PageModels;

public partial class JindungoAccessPageModel : ObservableObject
{
    public JindungoAccessPageModel()
    {
    }

    [RelayCommand]
    private async Task RequestAccess()
    {
        // Placeholder for requesting access
        if (Shell.Current != null)
        {
            await Shell.Current.DisplayAlert("Solicitar Acesso", "Sua solicitação de acesso ao Jindungo foi enviada!", "OK");
        }
    }
}
