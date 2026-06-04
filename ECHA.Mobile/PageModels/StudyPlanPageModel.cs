using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ECHA.Mobile.Models;
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
        LoadSugestoesCommand.Execute(null);
    }

    [RelayCommand]
    private async Task LoadSugestoesAsync()
    {
        Sugestoes = await _apiService.GetAsync<List<SugestaoEstudoDto>>("api/plano-estudo/sugestoes") ?? new();
    }
}
