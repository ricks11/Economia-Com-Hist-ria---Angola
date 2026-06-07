using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EconomiaComHistoria.Core.DTOs;
using ECHA.Mobile.Services;

namespace ECHA.Mobile.PageModels;

public partial class ProfilePageModel : ObservableObject
{
    private readonly IApiService _apiService;

    [ObservableProperty]
    private string _nome = string.Empty;

    [ObservableProperty]
    private string _escola = string.Empty;

    [ObservableProperty]
    private string _provincia = string.Empty;

    [ObservableProperty]
    private UserStatsDto? _stats;

    public ProfilePageModel(IApiService apiService)
    {
        _apiService = apiService;
        LoadProfile();
    }

    [RelayCommand]
    private async Task LoadStatsAsync()
    {
        Stats = await _apiService.GetAsync<UserStatsDto>("api/perfil/stats");
    }

    private void LoadProfile()
    {
        Nome = Preferences.Default.Get("nome", string.Empty);
        Escola = Preferences.Default.Get("escola", string.Empty);
        Provincia = Preferences.Default.Get("provincia", string.Empty);
    }

    [ObservableProperty]
    private string _codigoConvite = string.Empty;

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
}
