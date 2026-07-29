using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ECHA.Mobile.Services;

namespace ECHA.Mobile.PageModels;

public partial class StudyPlanPageModel : ObservableObject
{
    private readonly IApiService _apiService;

    public record SugestaoEstudo(string Titulo, string Prioridade, string TempoEstimado);

    [ObservableProperty]
    private List<SugestaoEstudo> _sugestoes = new();

    [ObservableProperty]
    private double _progressoGeral = 0.65;

    [ObservableProperty]
    private string _progressoTexto = "65% Concluído";

    public StudyPlanPageModel(IApiService apiService)
    {
        _apiService = apiService;
    }

    [RelayCommand]
    private void LoadSugestoes()
    {
        Sugestoes = new List<SugestaoEstudo>
        {
            new("Era Pré-Colonial", "Alta", "30 min"),
            new("Economia do Café", "Média", "45 min"),
            new("Independência e Guerra Civil", "Média", "1 hora")
        };
    }
}
