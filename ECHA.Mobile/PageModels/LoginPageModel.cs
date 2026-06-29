using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ECHA.Mobile.PageModels;

public partial class LoginPageModel : ObservableObject
{
    [ObservableProperty]
    private bool _isLoginMode = true;
    
    [ObservableProperty]
    private string _email = string.Empty;
    
    [ObservableProperty]
    private string _password = string.Empty;
    
    [ObservableProperty]
    private string _name = string.Empty;
    
    public LoginPageModel()
    {
        
    }
    
    [RelayCommand]
    private void ToggleMode()
    {
        IsLoginMode = !IsLoginMode;
    }
    
    [RelayCommand]
    private async Task Login()
    {
        await Shell.Current.GoToAsync("//MainPage");
    }
    
    [RelayCommand]
    private async Task ContinueAsGuest()
    {
        await Shell.Current.GoToAsync("//MainPage");
    }
}
