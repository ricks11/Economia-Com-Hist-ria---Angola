using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EconomiaComHistoria.Core.DTOs;
using ECHA.Mobile.Services;

namespace ECHA.Mobile.PageModels;

public partial class StudyPlanPageModel : ObservableObject
{
    private readonly IApiService _apiService;

    [ObservableProperty]
    private List<SugestaoEstudoDto> _sugestoes = new();

    public StudyPlanPageModel(IApiService apiService)
    {
        _apiService = apiService;
    }

    [RelayCommand]
    private async Task LoadSugestoesAsync()
    {
        Sugestoes = await _apiService.GetAsync<List<SugestaoEstudoDto>>("api/plano-estudo/sugestoes") ?? new();
    }
}
