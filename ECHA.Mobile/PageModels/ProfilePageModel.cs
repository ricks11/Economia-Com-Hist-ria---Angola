using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EconomiaComHistoria.Core.DTOs;
using ECHA.Mobile.Services;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;

namespace ECHA.Mobile.PageModels;

/// <summary>Estado e ações do perfil autenticado. Todos os dados persistentes vêm da API.</summary>
public partial class ProfilePageModel : ObservableObject
{
    private readonly IApiService _api;
    private readonly ITokenService _tokenService;
    private readonly CacheDbContext _cache;

    [ObservableProperty] private string _nome = "O teu perfil";
    [ObservableProperty] private string _email = string.Empty;
    [ObservableProperty] private string _telemovel = string.Empty;
    [ObservableProperty] private string _provincia = "Angola";
    [ObservableProperty] private string _tipo = "Estudante";
    [ObservableProperty] private string _escola = "Sem escola associada";
    [ObservableProperty] private int? _escolaId;
    [ObservableProperty] private int? _turmaId;
    [ObservableProperty] private int _streakAtual;
    [ObservableProperty] private int _pontosTotais;
    [ObservableProperty] private int _nivel = 1;
    [ObservableProperty] private int _pontosParaProximoNivel = 1000;
    [ObservableProperty] private double _percentagemNivel;
    [ObservableProperty] private int _offlineCount;
    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private ImageSource? _avatarImage;
    [ObservableProperty] private bool _hasAvatar;
    public ObservableCollection<BadgeConquistadoDto> Badges { get; } = [];

    public string DisplaySubtitle => $"{Tipo} · Nível {Nivel} · {Provincia}";
    public string StreakText => $"{StreakAtual} {(StreakAtual == 1 ? "dia" : "dias")}";
    public string PontosText => PontosTotais.ToString("N0");
    public string ProgressoText => $"{PercentagemNivel:0}% completo";
    public double ProgressFraction => Math.Clamp(PercentagemNivel / 100d, 0d, 1d);
    public string ObjetivoText => $"Faltam {PontosParaProximoNivel:N0} XP para o próximo nível";

    public ProfilePageModel(IApiService api, ITokenService tokenService, CacheDbContext cache)
    {
        _api = api;
        _tokenService = tokenService;
        _cache = cache;
    }

    public async Task LoadAsync()
    {
        if (IsBusy || !await _tokenService.IsAuthenticatedAsync()) return;

        IsBusy = true;
        try
        {
            var profile = await _api.AuthGetAsync<PerfilResponse>("api/perfil");
            if (profile is not null)
            {
                Nome = profile.Nome;
                Email = profile.Email;
                Telemovel = profile.Telemovel ?? string.Empty;
                Provincia = string.IsNullOrWhiteSpace(profile.Provincia) ? "Angola" : profile.Provincia;
                Escola = profile.EscolaNome ?? "Sem escola associada";
                EscolaId = profile.EscolaId;
                TurmaId = profile.TurmaId;
                Tipo = ToRole(profile.Tipo);
                SetAvatar(profile.AvatarConfig);
            }

            // O progresso é complementar: uma indisponibilidade temporária da BD não
            // deve impedir o utilizador de abrir o perfil ou terminar a sessão.
            try
            {
                var progress = await _api.AuthGetAsync<ProgressoUtilizadorDto>("api/perfil/progresso");
                if (progress is not null)
                {
                    PontosTotais = progress.PontosTotais;
                    StreakAtual = progress.StreakAtual;
                    Nivel = progress.Nivel;
                    PontosParaProximoNivel = progress.PontosParaProximoNivel;
                    PercentagemNivel = progress.PercentagemNivel;
                    Badges.Clear();
                    foreach (var badge in progress.Badges.Take(4)) Badges.Add(badge);
                }
            }
            catch (HttpRequestException) { }

            try { OfflineCount = await _cache.OfflineContents.CountAsync(); } catch { OfflineCount = 0; }
            NotifyComputed();
        }
        catch (UnauthorizedAccessException)
        {
            await EndSessionAsync();
        }
        catch (Exception)
        {
            await Shell.Current.DisplayAlert("Perfil", "Não foi possível atualizar o perfil. Verifique a ligação e tente novamente.", "OK");
        }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private async Task ChangeAvatarAsync()
    {
        try
        {
            var photo = await MediaPicker.Default.PickPhotoAsync(new MediaPickerOptions { Title = "Escolher fotografia de perfil" });
            if (photo is null) return;

            await using var source = await photo.OpenReadAsync();
            using var memory = new MemoryStream();
            await source.CopyToAsync(memory);
            if (memory.Length > 3_000_000)
            {
                await Shell.Current.DisplayAlert("Fotografia demasiado grande", "Escolha uma imagem com menos de 3 MB.", "OK");
                return;
            }

            var mime = string.IsNullOrWhiteSpace(photo.ContentType) ? "image/jpeg" : photo.ContentType;
            var avatar = $"data:{mime};base64,{Convert.ToBase64String(memory.ToArray())}";
            var response = await _api.AuthPutAsync<UpdateAvatarRequest, PerfilResponse>("api/perfil/avatar", new(avatar));
            SetAvatar(response?.AvatarConfig ?? avatar);
        }
        catch (Exception)
        {
            await Shell.Current.DisplayAlert("Fotografia", "Não foi possível guardar a fotografia. Tente novamente.", "OK");
        }
    }

    [RelayCommand]
    private async Task EditAccountAsync()
    {
        var newName = await Shell.Current.DisplayPromptAsync("Detalhes da conta", "Nome", "Guardar", "Cancelar", Nome, maxLength: 100);
        if (newName is null) return;
        var newProvince = await Shell.Current.DisplayPromptAsync("Detalhes da conta", "Província", "Guardar", "Cancelar", Provincia, maxLength: 100);
        if (newProvince is null) return;
        var newPhone = await Shell.Current.DisplayPromptAsync("Detalhes da conta", "Telemóvel (opcional)", "Guardar", "Cancelar", Telemovel, maxLength: 50, keyboard: Keyboard.Telephone);
        if (newPhone is null) return;

        try
        {
            var response = await _api.AuthPutAsync<UpdateProfileRequest, PerfilResponse>("api/perfil",
                new(newName.Trim(), newProvince.Trim(), newPhone.Trim(), EscolaId, TurmaId));
            if (response is not null)
            {
                Nome = response.Nome;
                Provincia = response.Provincia ?? "Angola";
                Telemovel = response.Telemovel ?? string.Empty;
                Preferences.Default.Set("user_name", Nome);
                NotifyComputed();
            }
            await Shell.Current.DisplayAlert("Perfil atualizado", "As alterações foram guardadas.", "OK");
        }
        catch (Exception)
        {
            await Shell.Current.DisplayAlert("Perfil", "Não foi possível guardar as alterações.", "OK");
        }
    }

    [RelayCommand]
    private async Task ChangePasswordAsync()
    {
        var current = await Shell.Current.DisplayPromptAsync("Alterar palavra-passe", "Palavra-passe atual", "Continuar", "Cancelar", keyboard: Keyboard.Text);
        if (current is null) return;
        var next = await Shell.Current.DisplayPromptAsync("Alterar palavra-passe", "Nova palavra-passe (mínimo 8 caracteres)", "Guardar", "Cancelar", keyboard: Keyboard.Text);
        if (next is null) return;
        if (next.Length < 8)
        {
            await Shell.Current.DisplayAlert("Palavra-passe", "A nova palavra-passe deve ter pelo menos 8 caracteres.", "OK");
            return;
        }

        try
        {
            await _api.AuthPutAsync<ChangePasswordRequest, MessageResponse>("api/perfil/password", new(current, next));
            await Shell.Current.DisplayAlert("Palavra-passe", "A palavra-passe foi alterada com sucesso.", "OK");
        }
        catch (Exception)
        {
            await Shell.Current.DisplayAlert("Palavra-passe", "Não foi possível alterar a palavra-passe. Confirme a palavra-passe atual.", "OK");
        }
    }

    [RelayCommand] private async Task ManageDownloadsAsync() => await Shell.Current.DisplayAlert("Downloads", $"Tem {OfflineCount} módulo(s) guardado(s) para uso offline.", "OK");
    [RelayCommand] private async Task GoAchievementsAsync() => await Shell.Current.GoToAsync("//AchievementsPage");
    [RelayCommand] private async Task SubscribeAsync() => await Shell.Current.DisplayAlert("Subscrição", "A subscrição estará disponível em breve.", "OK");

    [RelayCommand]
    private async Task LogoutAsync()
    {
        if (!await Shell.Current.DisplayAlert("Sair da conta", "Tem a certeza de que pretende terminar a sessão?", "Sair", "Cancelar")) return;
        await EndSessionAsync();
    }

    private async Task EndSessionAsync()
    {
        await _tokenService.RemoveTokenAsync();
        Preferences.Default.Remove("user_name");
        Preferences.Default.Remove("user_email");
        Preferences.Default.Remove("user_role");
        await Shell.Current.GoToAsync("//LoginPage");
    }

    private void SetAvatar(string? dataUrl)
    {
        if (string.IsNullOrWhiteSpace(dataUrl) || !dataUrl.Contains(',')) { AvatarImage = null; HasAvatar = false; return; }
        try
        {
            var bytes = Convert.FromBase64String(dataUrl[(dataUrl.IndexOf(',') + 1)..]);
            AvatarImage = ImageSource.FromStream(() => new MemoryStream(bytes));
            HasAvatar = true;
        }
        catch { AvatarImage = null; HasAvatar = false; }
    }

    private void NotifyComputed()
    {
        OnPropertyChanged(nameof(DisplaySubtitle)); OnPropertyChanged(nameof(StreakText));
        OnPropertyChanged(nameof(PontosText)); OnPropertyChanged(nameof(ProgressoText)); OnPropertyChanged(nameof(ObjetivoText));
        OnPropertyChanged(nameof(ProgressFraction));
    }

    private static string ToRole(int role) => role switch { 1 => "Estudante", 2 => "Professor", 3 => "Editor", 4 => "Administrador", _ => "Membro" };
    private sealed record PerfilResponse(string Nome, string Email, string? Telemovel, int Tipo, string? Provincia, int? EscolaId, string? EscolaNome, int? TurmaId, string? AvatarConfig);
    private sealed record UpdateProfileRequest(string Nome, string Provincia, string Telemovel, int? EscolaId, int? TurmaId);
    private sealed record UpdateAvatarRequest(string AvatarBase64);
    private sealed record ChangePasswordRequest(string PalavraPasseAtual, string NovaPalavraPasse);
    private sealed record MessageResponse(string Message);
}
