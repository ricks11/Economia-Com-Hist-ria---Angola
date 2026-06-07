using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EconomiaComHistoria.Core.DTOs;
using ECHA.Mobile.Services;
using System.Collections.ObjectModel;

namespace ECHA.Mobile.PageModels;

public partial class RankingPageModel : ObservableObject
{
    private readonly IApiService _apiService;

    [ObservableProperty]
    private ObservableCollection<RankingEntradaDto> _rankings = new();

    public RankingPageModel(IApiService apiService)
    {
        _apiService = apiService;
    }

    [RelayCommand]
    private async Task LoadRankingsAsync()
    {
        try
        {
            // Defaulting to Global/Mensal for mobile overview
            var endpoint = "api/ranking?tipo=Global&periodo=Mensal";
            var result = await _apiService.GetAsync<RankingResponseDto>(endpoint);
            
            Rankings.Clear();
            if (result?.Top100 != null)
            {
                foreach (var item in result.Top100)
                {
                    Rankings.Add(item);
                }
            }
        }
        catch (Exception ex)
        {
             System.Diagnostics.Debug.WriteLine($"Error loading rankings: {ex.Message}");
        }
    }
}
