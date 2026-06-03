using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ECHA.Mobile.Models;
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

    public ProfilePageModel(IApiService apiService)
    {
        _apiService = apiService;
        LoadProfile();
    }

    private void LoadProfile()
    {
        Nome = Preferences.Default.Get("nome", string.Empty);
        Escola = Preferences.Default.Get("escola", string.Empty);
        Provincia = Preferences.Default.Get("provincia", string.Empty);
    }

    [RelayCommand]
    private async Task SaveProfileAsync()
    {
        var perfil = new PerfilDto(Nome, Escola, Provincia);

        try
        {
            await _apiService.PostAsync<PerfilDto, object>("api/perfil/atualizar", perfil);
            
            Preferences.Default.Set("nome", Nome);
            Preferences.Default.Set("escola", Escola);
            Preferences.Default.Set("provincia", Provincia);
            
            await Shell.Current.DisplayAlert("Sucesso", "Perfil atualizado!", "OK");
        }
        catch
        {
            await Shell.Current.DisplayAlert("Erro", "Falha ao atualizar perfil.", "OK");
        }
    }
}
