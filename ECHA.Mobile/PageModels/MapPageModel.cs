using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ECHA.Mobile.Services;

namespace ECHA.Mobile.PageModels;

public partial class MapPageModel : ObservableObject
{
    private readonly IApiService _apiService;

    public record ProvinciaProgresso(string NomeProvincia, double PercentualExplorado);

    [ObservableProperty]
    private List<ProvinciaProgresso> _progresso = new();

    public MapPageModel(IApiService apiService)
    {
        _apiService = apiService;
    }

    [RelayCommand]
    private void LoadProgresso()
    {
        // Placeholder data
        Progresso = new List<ProvinciaProgresso>
        {
            new("Luanda", 0.8),
            new("Benguela", 0.5),
            new("Huíla", 0.3)
        };
    }
}
