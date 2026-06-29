using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EconomiaComHistoria.Core.DTOs;
using ECHA.Mobile.Services;

namespace ECHA.Mobile.PageModels;

public partial class TurmaRankingPageModel : ObservableObject
{
    private readonly IApiService _apiService;

    [ObservableProperty]
    private List<RankingEntradaDto> _ranking = new();

    public TurmaRankingPageModel(IApiService apiService)
    {
        _apiService = apiService;
    }

    [RelayCommand]
    private async Task LoadRankingAsync()
    {
        // For now, just use a placeholder
        var response = await _apiService.GetAsync<RankingResponseDto>("api/ranking/geral");
        Ranking = response?.Top100 ?? new();
    }
}
