using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ECHA.Mobile.Data;
using ECHA.Mobile.Services;
using EconomiaComHistoria.Core.DTOs;

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
            var mapaDto = await _apiService.GetAsync<MapaProgressoDto>("api/mapa/progresso");

            if (mapaDto?.Provincias is null)
            {
                IsLoading = false;
                return;
            }

            var progressoDict = mapaDto.Provincias
                .ToDictionary(p => p.ProvinciaId, p => p.PercentualExplorado, StringComparer.OrdinalIgnoreCase);

            var items = AngolaMapData.AllProvinces.Select(p =>
            {
                var percentual = progressoDict.TryGetValue(p.Id, out var val) ? val : 0.0;
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
            PercentualGeral = items.Count > 0 ? items.Average(i => i.PercentualExplorado) : 0.0;
            PercentualGeralTexto = $"{(int)(PercentualGeral * 100)}%";
        }
        catch (Exception)
        {
            // Falha silenciosa — mapa fica com 0% em todas as províncias
            var items = AngolaMapData.AllProvinces.Select(p =>
            {
                var item = new ProvinciaMapItem
                {
                    Id = p.Id,
                    NomeProvincia = p.Nome,
                    PathData = p.PathData,
                    PercentualExplorado = 0.0
                };
                item.UpdateCor();
                return item;
            }).ToList();
            Provincias = new ObservableCollection<ProvinciaMapItem>(items);
            PercentualGeralTexto = "0%";
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
