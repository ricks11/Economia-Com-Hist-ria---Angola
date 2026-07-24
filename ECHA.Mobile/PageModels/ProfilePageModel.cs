using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EconomiaComHistoria.Core.DTOs;
using ECHA.Mobile.Services;

namespace ECHA.Mobile.PageModels;

public partial class ProfilePageModel : ObservableObject
{
    private readonly IApiService _apiService;

    [ObservableProperty]
    private string _nome = "Kátia dos Santos";

    [ObservableProperty]
    private string _escola = string.Empty;

    [ObservableProperty]
    private string _provincia = "Luanda";

    [ObservableProperty]
    private UserStatsDto? _stats;

    [ObservableProperty]
    private string _codigoConvite = string.Empty;

    public string DisplayName => string.IsNullOrWhiteSpace(Nome) ? "Kátia dos Santos" : Nome;
    public string DisplaySubtitle => $"Curadora de Nível {(Stats?.Nivel ?? 4)} • {(string.IsNullOrWhiteSpace(Provincia) ? "Luanda" : Provincia)}";

    public ProfilePageModel(IApiService apiService)
    {
        _apiService = apiService;
        LoadProfile();
    }

    [RelayCommand]
    private async Task LoadStatsAsync()
    {
        Stats = await _apiService.GetAsync<UserStatsDto>("api/perfil/stats");
        OnPropertyChanged(nameof(DisplaySubtitle));
    }

    private void LoadProfile()
    {
        var saved = Preferences.Default.Get("nome", string.Empty);
        if (!string.IsNullOrWhiteSpace(saved)) Nome = saved;
        Escola = Preferences.Default.Get("escola", string.Empty);
        var prov = Preferences.Default.Get("provincia", string.Empty);
        if (!string.IsNullOrWhiteSpace(prov)) Provincia = prov;
    }

    [RelayCommand]
    private async Task AssociarEscolaAsync()
    {
        try
        {
            await _apiService.PostAsync<AssociarAlunoDto, object>("api/institucional/associar", new AssociarAlunoDto(CodigoConvite));
            await Shell.Current.DisplayAlert("Sucesso", "Associação concluída!", "OK");
        }
        catch
        {
            await Shell.Current.DisplayAlert("Erro", "Falha na associação.", "OK");
        }
    }

    [RelayCommand]
    private async Task SaveProfile()
    {
        Preferences.Default.Set("nome", Nome);
        Preferences.Default.Set("escola", Escola);
        Preferences.Default.Set("provincia", Provincia);
        await Shell.Current.DisplayAlert("Sucesso", "Perfil guardado!", "OK");
    }

    [RelayCommand]
    private async Task GoAchievements()
    {
        await Shell.Current.GoToAsync("//AchievementsPage");
    }
}
