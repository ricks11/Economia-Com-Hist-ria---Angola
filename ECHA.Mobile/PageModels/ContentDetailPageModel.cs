using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EconomiaComHistoria.Core.DTOs;
using ECHA.Mobile.Services;

namespace ECHA.Mobile.PageModels;

public partial class ContentDetailPageModel : ObservableObject, IQueryAttributable
{
    private readonly IApiService _apiService;

    [ObservableProperty]
    private ConteudoResponseDto? _conteudo;

    public ContentDetailPageModel(IApiService apiService)
    {
        _apiService = apiService;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("Conteudo", out var conteudo))
        {
            Conteudo = (ConteudoResponseDto)conteudo;
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
        var novoEstado = !Conteudo.EhFavorito;
        
        // Persist locally
        Preferences.Default.Set($"favorito_{Conteudo.Id}", novoEstado);
        
        // Update local object
        Conteudo = Conteudo with { EhFavorito = novoEstado };

        // Sync with API
        try
        {
            await _apiService.PostAsync<object, object>($"api/conteudos/{Conteudo.Id}/favorito", new { });
        }
        catch
        {
            // Revert on failure
            Conteudo = Conteudo with { EhFavorito = !novoEstado };
            Preferences.Default.Set($"favorito_{Conteudo.Id}", !novoEstado);
        }
    }
}
