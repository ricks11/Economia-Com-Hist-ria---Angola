using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ECHA.Mobile.Models;
using ECHA.Mobile.Services;

namespace ECHA.Mobile.PageModels;

public partial class ExplorePageModel : ObservableObject
{
    private readonly IApiService _apiService;

    [ObservableProperty]
    private bool _isBusy;

    public ObservableCollection<ConteudoDto> Conteudos { get; } = new();

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
            var conteudos = await _apiService.GetAsync<List<ConteudoDto>>("api/conteudos");
            
            Conteudos.Clear();
            if (conteudos != null)
            {
                foreach (var item in conteudos)
                {
                    Conteudos.Add(item);
                }
            }
        }
        finally
        {
            IsBusy = false;
        }
    }
}
