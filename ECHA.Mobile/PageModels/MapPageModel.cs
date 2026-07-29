using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ECHA.Mobile.Data;
using ECHA.Mobile.Services;

namespace ECHA.Mobile.PageModels;

public partial class MapPageModel : ObservableObject
{
    private readonly IApiService _apiService;

    [ObservableProperty]
    private ObservableCollection<ProvinciaMapItem> _provincias = new();

    [ObservableProperty]
    private ProvinciaMapItem? _selectedProvincia;

    [ObservableProperty]
    private bool _hasSelection;

    [ObservableProperty]
    private double _percentualGeral;

    [ObservableProperty]
    private string _percentualGeralTexto = "0%";

    [ObservableProperty]
    private bool _isLoading;

    public MapPageModel(IApiService apiService)
    {
        _apiService = apiService;
    }

    [RelayCommand]
    private async Task LoadProgresso()
    {
        if (IsLoading) return;
        IsLoading = true;

        try
        {
            // Dados de demonstração – substituir pela chamada à API real
            var progressoSimulado = new Dictionary<string, double>
            {
                { "AO-LUA", 0.85 },  // Luanda
                { "AO-BGO", 0.60 },  // Bengo
                { "AO-CNO", 0.45 },  // Cuanza Norte
                { "AO-CUS", 0.30 },  // Cuanza Sul
                { "AO-MAL", 0.20 },  // Malanje
                { "AO-UIG", 0.15 },  // Uíge
                { "AO-ZAI", 0.10 },  // Zaire
                { "AO-CAB", 0.05 },  // Cabinda
                { "AO-BGU", 0.70 },  // Benguela
                { "AO-HUA", 0.40 },  // Huambo
                { "AO-HUI", 0.25 },  // Huíla
                { "AO-NAM", 0.00 },  // Namibe
                { "AO-CNN", 0.00 },  // Cunene
                { "AO-CCU", 0.00 },  // Cuando-Cubango
                { "AO-MOX", 0.00 },  // Moxico
                { "AO-LSU", 0.00 },  // Lunda Sul
                { "AO-LNO", 0.00 },  // Lunda Norte
                { "AO-BIE", 0.00 },  // Bié
            };

            var items = AngolaMapData.AllProvinces.Select(p =>
            {
                var percentual = progressoSimulado.TryGetValue(p.Id, out var val) ? val : 0.0;
                var item = new ProvinciaMapItem
                {
                    Id = p.Id,
                    NomeProvincia = p.Nome,
                    PathData = p.PathData,
                    PercentualExplorado = percentual
                };
                item.UpdateCor();
                return item;
            }).ToList();

            Provincias = new ObservableCollection<ProvinciaMapItem>(items);

            // Calcular percentual geral
            PercentualGeral = items.Count > 0 ? items.Average(i => i.PercentualExplorado) : 0.0;
            PercentualGeralTexto = $"{(int)(PercentualGeral * 100)}%";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void SelectProvincia(ProvinciaMapItem? provincia)
    {
        if (SelectedProvincia is not null)
            SelectedProvincia.IsSelected = false;

        if (provincia is not null && provincia == SelectedProvincia)
        {
            // Desseleccionar ao tocar na mesma província
            SelectedProvincia = null;
            HasSelection = false;
        }
        else
        {
            SelectedProvincia = provincia;
            if (SelectedProvincia is not null)
                SelectedProvincia.IsSelected = true;
            HasSelection = provincia is not null;
        }
    }

    [RelayCommand]
    private async Task ExplorarProvincia()
    {
        if (SelectedProvincia is null) return;
        // Navegar para a página de conteúdo da província selecionada
        await Shell.Current.GoToAsync($"ExplorePage?provinciaId={SelectedProvincia.Id}");
    }
}
