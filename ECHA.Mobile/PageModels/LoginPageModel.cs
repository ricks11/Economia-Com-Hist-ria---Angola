using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EconomiaComHistoria.Core.DTOs;
using ECHA.Mobile.Services;

namespace ECHA.Mobile.PageModels;

public partial class LoginPageModel : ObservableObject
{
    private readonly IApiService _apiService;

    public LoginPageModel(IApiService apiService) => _apiService = apiService;
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SubmitButtonText))]
    private bool _isLoginMode = true;

    [ObservableProperty]
    private string _email = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private string _name = string.Empty;

    public string SubmitButtonText => IsLoginMode ? "Entrar no Arquivo →" : "Criar Conta →";

    [RelayCommand]
    private void ToggleMode() => IsLoginMode = !IsLoginMode;

    [RelayCommand]
    private void SetLoginMode() => IsLoginMode = true;

    [RelayCommand]
    private void SetRegisterMode() => IsLoginMode = false;

    [RelayCommand]
    private async Task Login()
    {
        try
        {
            AuthResponseDto? response = IsLoginMode
                ? await _apiService.PostAsync<LoginRequestDto, AuthResponseDto>("api/auth/login", new(Email, Password))
                : await _apiService.PostAsync<RegisterRequestDto, AuthResponseDto>("api/auth/register", new(Email, Password, Name, null));

            if (response is null) throw new HttpRequestException("A API nao devolveu uma sessao valida.");
            _apiService.SetAccessToken(response.AccessToken);
            Preferences.Default.Set("access_token", response.AccessToken);
            Preferences.Default.Set("user_name", response.Nome);
            await Shell.Current.GoToAsync("//MainPage");
        }
        catch (Exception ex)
        {
            await AppShell.DisplaySnackbarAsync(ex.Message.Contains("409") ? "Este email ja esta registado." : "Nao foi possivel autenticar. Confirme os dados e a API.");
        }
    }

    [RelayCommand]
    private async Task ContinueAsGuest()
    {
        await Shell.Current.GoToAsync("//MainPage");
    }
}
