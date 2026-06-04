using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ECHA.Mobile.Models;
using ECHA.Mobile.Services;

namespace ECHA.Mobile.PageModels;

public partial class MapPageModel : ObservableObject
{
    private readonly IApiService _apiService;

    [ObservableProperty]
    private List<ProgressoProvinciaDto> _progresso = new();

    public MapPageModel(IApiService apiService)
    {
        _apiService = apiService;
        LoadProgressoCommand.Execute(null);
    }

    [RelayCommand]
    private async Task LoadProgressoAsync()
    {
        Progresso = await _apiService.GetAsync<List<ProgressoProvinciaDto>>("api/gamificacao/mapa") ?? new();
    }
}
