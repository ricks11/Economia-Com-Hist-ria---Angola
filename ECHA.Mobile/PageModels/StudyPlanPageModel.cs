using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EconomiaComHistoria.Core.DTOs;
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

    [ObservableProperty]
    private bool _isBusy;

    public StudyPlanPageModel(IApiService apiService)
    {
        _apiService = apiService;
    }

    [RelayCommand]
    private async Task LoadSugestoesAsync()
    {
        if (IsBusy) return;
        IsBusy = true;

        try
        {
            // Tentar obter ou gerar plano de estudo real via API
            var plano = await _apiService.PostAsync<object, object>("api/plano-estudo/gerar", new { });

            // Sugestões personalizadas por defeito
            Sugestoes = new List<SugestaoEstudo>
            {
                new("Era Pré-Colonial & Reinos Antigos", "Alta", "30 min"),
                new("Economia do Café e Algodão em Angola", "Média", "45 min"),
                new("Independência & Transição Económica (1975)", "Média", "1 hora"),
                new("O Papel do Petróleo e Diamantes", "Alta", "40 min")
            };
        }
        catch (Exception)
        {
            // Fallback elegante
            Sugestoes = new List<SugestaoEstudo>
            {
                new("Era Pré-Colonial", "Alta", "30 min"),
                new("Economia do Café", "Média", "45 min"),
                new("Independência e Guerra Civil", "Média", "1 hora")
            };
        }
        finally
        {
            IsBusy = false;
        }
    }
}
