using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ECHA.Mobile.Services;

namespace ECHA.Mobile.PageModels;

public partial class LoginPageModel : ObservableObject
{
    private readonly IApiService _apiService;
    private readonly ITokenService _tokenService;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SubmitButtonText))]
    private bool _isLoginMode = true;

    /// <summary>Texto do botão de submissão — muda conforme o modo (Login / Registo).</summary>
    public string SubmitButtonText => IsLoginMode ? "Entrar" : "Criar Conta";

    [ObservableProperty]
    private string _email = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private bool _isBusy = false;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    public LoginPageModel(IApiService apiService, ITokenService tokenService)
    {
        _apiService = apiService;
        _tokenService = tokenService;
    }

    [RelayCommand]
    private void ToggleMode()
    {
        IsLoginMode = !IsLoginMode;
        ErrorMessage = string.Empty;
    }

    [RelayCommand]
    private async Task Login()
    {
        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Por favor, preencha o email e a senha.";
            return;
        }

        IsBusy = true;
        ErrorMessage = string.Empty;

        try
        {
            // Chama o endpoint público de login (sem token) e recebe o JWT
            var response = await _apiService.PostAsync<LoginRequest, LoginResponse>(
                "api/auth/login",
                new LoginRequest(Email, Password));

            if (response?.Token is null)
            {
                ErrorMessage = "Resposta inválida do servidor. Tente novamente.";
                return;
            }

            // Guarda o token de forma segura no Keystore/Keychain nativo
            await _tokenService.SaveTokenAsync(response.Token);

            await Shell.Current.GoToAsync("//MainPage");
        }
        catch (UnauthorizedAccessException)
        {
            ErrorMessage = "Email ou senha incorretos.";
        }
        catch (HttpRequestException ex)
        {
            ErrorMessage = $"Erro de ligação: {ex.Message}";
        }
        catch (Exception)
        {
            ErrorMessage = "Ocorreu um erro inesperado. Verifique a sua ligação à internet.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task ContinueAsGuest()
    {
        await Shell.Current.GoToAsync("//MainPage");
    }

    // ─────────────────────────────────────────
    // MODELOS DE TRANSFERÊNCIA
    // ─────────────────────────────────────────

    private record LoginRequest(string Email, string Password);

    private record LoginResponse(string Token, string Role, string NomeCompleto);
}

