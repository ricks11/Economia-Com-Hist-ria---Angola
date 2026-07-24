using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EconomiaComHistoria.Core.DTOs;
using ECHA.Mobile.Services;
using System.Collections.ObjectModel;

namespace ECHA.Mobile.PageModels;

public partial class RankingRowCard : ObservableObject
{
    public string Position { get; set; } = "";
    public string Name { get; set; } = "";
    public string Location { get; set; } = "";
    public string Points { get; set; } = "";
    public bool IsCurrentUser { get; set; }
}

public partial class RankingPageModel : ObservableObject
{
    private readonly IApiService _apiService;

    [ObservableProperty]
    private ObservableCollection<RankingEntradaDto> _rankings = new();

    public ObservableCollection<RankingRowCard> DisplayRankings { get; } = new();

    public RankingPageModel(IApiService apiService)
    {
        _apiService = apiService;
        SeedDesignRankings();
    }

    private void SeedDesignRankings()
    {
        DisplayRankings.Clear();
        DisplayRankings.Add(new RankingRowCard { Position = "4", Name = "Marta Sebastião", Location = "Luanda, Angola", Points = "9,840" });
        DisplayRankings.Add(new RankingRowCard { Position = "12", Name = "Tu (Francisco)", Location = "Sua posição atual", Points = "4,210", IsCurrentUser = true });
        DisplayRankings.Add(new RankingRowCard { Position = "5", Name = "Beatriz Costa", Location = "Benguela", Points = "8,200" });
        DisplayRankings.Add(new RankingRowCard { Position = "6", Name = "André Vunge", Location = "Huambo", Points = "7,950" });
        DisplayRankings.Add(new RankingRowCard { Position = "7", Name = "Teresa G.", Location = "Cabinda", Points = "7,100" });
    }

    [RelayCommand]
    private async Task LoadRankingsAsync()
    {
        try
        {
            var endpoint = "api/ranking?tipo=Global&periodo=Mensal";
            var result = await _apiService.GetAsync<RankingResponseDto>(endpoint);
            Rankings.Clear();
            if (result?.Top100 != null)
            {
                foreach (var item in result.Top100)
                    Rankings.Add(item);

                if (result.Top100.Count > 0)
                {
                    DisplayRankings.Clear();
                    var i = 1;
                    foreach (var item in result.Top100.Take(10))
                    {
                        DisplayRankings.Add(new RankingRowCard
                        {
                            Position = i.ToString(),
                            Name = item.NomeUtilizador ?? "Utilizador",
                            Location = "Angola",
                            Points = item.Pontos.ToString("N0")
                        });
                        i++;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading rankings: {ex.Message}");
        }
    }
}
