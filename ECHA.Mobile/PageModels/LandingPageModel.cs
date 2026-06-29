using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ECHA.Mobile.PageModels;

public partial class LandingPageModel : ObservableObject
{
    [ObservableProperty]
    private string _welcomeMessage = "Desvende a História Económica de Angola.";

    [ObservableProperty]
    private string _subtitle = "Uma jornada pedagógica premium através dos marcos que moldaram a nossa economia. Aprenda, explore e conecte-se com as raízes do nosso futuro.";

    public LandingPageModel()
    {
        
    }
    
    [RelayCommand]
    private async Task StartJourney()
    {
        await Shell.Current.GoToAsync("//LoginPage");
    }
    
    [RelayCommand]
    private async Task CreateAccount()
    {
        await Shell.Current.GoToAsync("//LoginPage");
    }
}
