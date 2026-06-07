using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EconomiaComHistoria.Core.DTOs;
using EconomiaComHistoria.Core.Helpers;
using ECHA.Mobile.Services;

namespace ECHA.Mobile.PageModels;

public partial class ExplorePageModel : ObservableObject
{
    private readonly IApiService _apiService;

    [ObservableProperty]
    private bool _isBusy;

    public ObservableCollection<ConteudoResponseDto> Conteudos { get; } = new();

    public ExplorePageModel(IApiService apiService)
    {
        _apiService = apiService;
    }

    [RelayCommand]
    private async Task LoadConteudosAsync()
    {
        if (IsBusy) return;

        IsBusy = true;
        try
        {
            var pagedResult = await _apiService.GetAsync<PagedResult<ConteudoResponseDto>>("api/conteudos");
            
            Conteudos.Clear();
            if (pagedResult?.Items != null)
            {
                foreach (var item in pagedResult.Items)
                {
                    Conteudos.Add(item);
                }
            }
        }
        catch (Exception ex)
        {
            // Log or handle error
            System.Diagnostics.Debug.WriteLine($"Error loading conteudos: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }
}
