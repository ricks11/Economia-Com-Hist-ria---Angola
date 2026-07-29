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



    [ObservableProperty]
    private string _email = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private string _confirmPassword = string.Empty;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _phone = string.Empty;

    [ObservableProperty]
    private bool _acceptedTerms;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SubmitCommand))]
    private bool _isBusy;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasErrorMessage))]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasInfoMessage))]
    private string _infoMessage = string.Empty;

    public string SubmitButtonText => IsLoginMode ? "Entrar" : "Criar conta";

    public bool HasErrorMessage => !string.IsNullOrWhiteSpace(ErrorMessage);

    public bool HasInfoMessage => !string.IsNullOrWhiteSpace(InfoMessage);

    public LoginPageModel(IApiService apiService, ITokenService tokenService)
    {
        _apiService = apiService;
        _tokenService = tokenService;
    }

    [RelayCommand]
    private void SetLoginMode()
    {
        IsLoginMode = true;
        ClearMessages();
    }

    [RelayCommand]
    private void SetRegisterMode()
    {
        IsLoginMode = false;
        ClearMessages();
    }

    [RelayCommand]
    private void ToggleMode()
    {
        IsLoginMode = !IsLoginMode;
        ClearMessages();
    }

    [RelayCommand(CanExecute = nameof(CanSubmit))]
    private async Task Submit()
    {
        if (IsLoginMode)
            await LoginAsync();
        else
            await RegisterAsync();
    }

    [RelayCommand]
    private async Task RecoverAccess()
    {
        ClearMessages();

        if (string.IsNullOrWhiteSpace(Email))
        {
            ErrorMessage = "Introduza o email para recuperar o acesso.";
            return;
        }

        IsBusy = true;
        try
        {
            await _apiService.PostAsync<ForgotPasswordRequest, MessageResponse>(
                "api/auth/forgot-password",
                new ForgotPasswordRequest(Email.Trim()));

            InfoMessage = "Se o email estiver registado, receberá um link de recuperação em breve.";
        }
        catch (HttpRequestException ex)
        {
            ErrorMessage = $"Erro de ligação: {ex.Message}";
        }
        catch (Exception)
        {
            ErrorMessage = "Não foi possível solicitar a recuperação. Tente novamente.";
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

    private bool CanSubmit() => !IsBusy;

    private async Task LoginAsync()
    {
        ClearMessages();

        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Por favor, preencha o email e a senha.";
            return;
        }

        IsBusy = true;
        try
        {
            var response = await _apiService.PostAsync<LoginRequest, AuthResponse>(
                "api/auth/login",
                new LoginRequest(Email.Trim(), Password));

            if (string.IsNullOrWhiteSpace(response?.AccessToken))
            {
                ErrorMessage = "Resposta inválida do servidor. Tente novamente.";
                return;
            }

            await PersistSessionAsync(response);
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

    private async Task RegisterAsync()
    {
        ClearMessages();

        if (string.IsNullOrWhiteSpace(Name) ||
            string.IsNullOrWhiteSpace(Email) ||
            string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Preencha nome, email e palavra-passe.";
            return;
        }

        if (Password.Length < 8)
        {
            ErrorMessage = "A palavra-passe deve ter no mínimo 8 caracteres.";
            return;
        }

        if (!string.Equals(Password, ConfirmPassword, StringComparison.Ordinal))
        {
            ErrorMessage = "As palavras-passe não coincidem.";
            return;
        }

        if (!AcceptedTerms)
        {
            ErrorMessage = "Tem de aceitar os Termos de Utilização.";
            return;
        }

        IsBusy = true;
        try
        {
            var telemovel = string.IsNullOrWhiteSpace(Phone) ? null : Phone.Trim();
            var response = await _apiService.PostAsync<RegisterRequest, AuthResponse>(
                "api/auth/register",
                new RegisterRequest(Email.Trim(), Password, Name.Trim(), telemovel));

            if (string.IsNullOrWhiteSpace(response?.AccessToken))
            {
                ErrorMessage = "Resposta inválida do servidor. Tente novamente.";
                return;
            }

            await PersistSessionAsync(response);
            await Shell.Current.GoToAsync("//MainPage");
        }
        catch (HttpRequestException ex) when (ex.Message.Contains("409"))
        {
            ErrorMessage = "Já existe uma conta com este email.";
        }
        catch (HttpRequestException ex)
        {
            ErrorMessage = $"Erro de ligação: {ex.Message}";
        }
        catch (Exception)
        {
            ErrorMessage = "Não foi possível criar a conta. Tente novamente.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task PersistSessionAsync(AuthResponse response)
    {
        await _tokenService.SaveTokenAsync(response.AccessToken);
        Preferences.Default.Set("user_name", response.Nome);
        Preferences.Default.Set("user_email", response.Email);
        Preferences.Default.Set("user_role", response.Tipo);
    }

    private void ClearMessages()
    {
        ErrorMessage = string.Empty;
        InfoMessage = string.Empty;
    }

    private record LoginRequest(string Email, string Password);
    private record RegisterRequest(string Email, string Password, string Nome, string? Telemovel);
    private record ForgotPasswordRequest(string Email);
    private record MessageResponse(string Message);
    private record AuthResponse(
        int Id,
        string Email,
        string Nome,
        string AccessToken,
        string RefreshToken,
        DateTime ExpiresIn,
        string Tipo);
}
