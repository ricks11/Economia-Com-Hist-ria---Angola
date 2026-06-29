using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EconomiaComHistoria.Core.DTOs;
using ECHA.Mobile.Services;

namespace ECHA.Mobile.PageModels;

public partial class TeacherDashboardPageModel : ObservableObject
{
    private readonly IApiService _apiService;

    [ObservableProperty]
    private List<RankingEntradaDto> _alunos = new();

    public TeacherDashboardPageModel(IApiService apiService)
    {
        _apiService = apiService;
    }

    [RelayCommand]
    private async Task LoadAlunosAsync()
    {
        // For now, just use a placeholder
        var response = await _apiService.GetAsync<RankingResponseDto>("api/ranking/geral");
        Alunos = response?.Top100 ?? new();
    }
}
