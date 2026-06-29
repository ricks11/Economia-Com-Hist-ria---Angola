using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EconomiaComHistoria.Core.DTOs;
using ECHA.Mobile.Services;

namespace ECHA.Mobile.PageModels;

public partial class SearchResultsPageModel : ObservableObject
{
    private readonly IApiService _apiService;

    public record SearchResult(string Title, string Description);

    [ObservableProperty]
    private string _searchQuery = string.Empty;

    [ObservableProperty]
    private List<SearchResult> _searchResults = new()
    {
        new SearchResult("Ciclo do Café (1870-1970)", "Análise da transformação económica."),
        new SearchResult("Relatório Porto Luanda 1952", "Documento original digitalizado."),
        new SearchResult("História do Diamante em Lunda", "Exploração económica e impacto social.")
    };

    public SearchResultsPageModel(IApiService apiService)
    {
        _apiService = apiService;
    }

    [RelayCommand]
    private async Task Search()
    {
        // For now, just keep placeholder results
    }

    [RelayCommand]
    private async Task GoToResult(SearchResult result)
    {
        // For now, just show alert
        if (Application.Current?.MainPage != null)
        {
            await Application.Current.MainPage.DisplayAlert("Abrir", $"A abrir: {result.Title}", "OK");
        }
    }
}
