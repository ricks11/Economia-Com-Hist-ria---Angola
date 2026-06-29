using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ECHA.Mobile.Services;

namespace ECHA.Mobile.PageModels;

public partial class StudyPlanPageModel : ObservableObject
{
    private readonly IApiService _apiService;

    public record SugestaoEstudo(string Titulo, string Prioridade);

    [ObservableProperty]
    private List<SugestaoEstudo> _sugestoes = new();

    public StudyPlanPageModel(IApiService apiService)
    {
        _apiService = apiService;
    }

    [RelayCommand]
    private void LoadSugestoes()
    {
        // Placeholder data
        Sugestoes = new List<SugestaoEstudo>
        {
            new("Era Pré-Colonial", "Alta"),
            new("Economia do Café", "Média"),
            new("Independência", "Baixa")
        };
    }
}
