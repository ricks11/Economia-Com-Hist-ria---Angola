using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ECHA.Mobile.Models;
using ECHA.Mobile.Services;

namespace ECHA.Mobile.PageModels;

public partial class ContentDetailPageModel : ObservableObject, IQueryAttributable
{
    private readonly IApiService _apiService;

    [ObservableProperty]
    private ConteudoDto? _conteudo;

    public ContentDetailPageModel(IApiService apiService)
    {
        _apiService = apiService;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("Conteudo", out var conteudo))
        {
            Conteudo = (ConteudoDto)conteudo;
        }
    }

    [RelayCommand]
    private async Task TrackViewAsync()
    {
        if (Conteudo == null) return;

        try
        {
            await _apiService.PostAsync<object, object>($"api/conteudos/{Conteudo.Id}/visualizacao", new { });
        }
        catch
        {
            // Silently fail view tracking
        }
    }

    [RelayCommand]
    private async Task ToggleFavoritoAsync()
    {
        if (Conteudo == null) return;

        // Toggle state locally
        var novoEstado = !Conteudo.IsFavorito;
        
        // Persist locally
        Preferences.Default.Set($"favorito_{Conteudo.Id}", novoEstado);
        
        // Update local object
        Conteudo = Conteudo with { IsFavorito = novoEstado };

        // Sync with API
        try
        {
            await _apiService.PostAsync<object, object>($"api/conteudos/{Conteudo.Id}/favorito", new { IsFavorito = novoEstado });
        }
        catch
        {
            // Revert on failure
            Conteudo = Conteudo with { IsFavorito = !novoEstado };
            Preferences.Default.Set($"favorito_{Conteudo.Id}", !novoEstado);
        }
    }
}
