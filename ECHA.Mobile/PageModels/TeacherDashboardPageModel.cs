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
    
    [ObservableProperty]
    private bool _isBusy;
    
    [ObservableProperty]
    private string _totalAlunos = "42";
    
    [ObservableProperty]
    private string _mediaTurma = "14.5";
    
    [ObservableProperty]
    private string _quizzesAtivos = "3";

    public TeacherDashboardPageModel(IApiService apiService)
    {
        _apiService = apiService;
    }

    [RelayCommand]
    private async Task LoadDashboardAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        
        try 
        {
            var response = await _apiService.GetAsync<RankingResponseDto>("api/ranking/geral");
            Alunos = response?.Top100?.Take(5).ToList() ?? new();
        }
        finally
        {
            IsBusy = false;
        }
    }
    
    [RelayCommand]
    private async Task NavigateToRankingAsync()
    {
        await Shell.Current.GoToAsync("TurmaRankingPage");
    }
}
