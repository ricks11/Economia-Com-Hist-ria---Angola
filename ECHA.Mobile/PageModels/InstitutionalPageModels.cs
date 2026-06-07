using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EconomiaComHistoria.Core.DTOs;
using ECHA.Mobile.Services;

namespace ECHA.Mobile.PageModels;

public partial class TurmaRankingPageModel : ObservableObject
{
    private readonly IApiService _apiService;

    [ObservableProperty]
    private List<TurmaRankingDto> _ranking = new();

    public TurmaRankingPageModel(IApiService apiService)
    {
        _apiService = apiService;
    }

    [RelayCommand]
    private async Task LoadRankingAsync()
    {
        Ranking = await _apiService.GetAsync<List<TurmaRankingDto>>("api/institucional/turma/ranking") ?? new();
    }
}

public partial class TeacherDashboardPageModel : ObservableObject
{
    private readonly IApiService _apiService;

    [ObservableProperty]
    private List<RelatorioProgressoDto> _alunos = new();

    public TeacherDashboardPageModel(IApiService apiService)
    {
        _apiService = apiService;
    }

    [RelayCommand]
    private async Task LoadAlunosAsync()
    {
        Alunos = await _apiService.GetAsync<List<RelatorioProgressoDto>>("api/institucional/professor/turma") ?? new();
    }
}
