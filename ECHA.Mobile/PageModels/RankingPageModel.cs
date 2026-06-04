using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ECHA.Mobile.Models;
using ECHA.Mobile.Services;

namespace ECHA.Mobile.PageModels;

public partial class RankingPageModel : ObservableObject
{
    private readonly IApiService _apiService;

    [ObservableProperty]
    private List<RankingItemDto> _rankings = new();

    public RankingPageModel(IApiService apiService)
    {
        _apiService = apiService;
        LoadRankingsCommand.Execute(null);
    }

    [RelayCommand]
    private async Task LoadRankingsAsync(string? filter = null)
    {
        var endpoint = filter != null ? $"api/rankings?filtro={filter}" : "api/rankings";
        Rankings = await _apiService.GetAsync<List<RankingItemDto>>(endpoint) ?? new();
    }
}
