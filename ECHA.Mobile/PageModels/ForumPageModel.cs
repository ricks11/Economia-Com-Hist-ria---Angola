using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EconomiaComHistoria.Core.DTOs;
using ECHA.Mobile.Services;
using System.Collections.ObjectModel;

namespace ECHA.Mobile.PageModels;

public partial class ForumPageModel : ObservableObject
{
    private readonly IApiService _apiService;

    [ObservableProperty]
    private bool _isBusy;

    public ObservableCollection<TopicoForumDto> Topicos { get; } = new();

    public ForumPageModel(IApiService apiService)
    {
        _apiService = apiService;
    }

    [RelayCommand]
    private async Task LoadTopicosAsync()
    {
        if (IsBusy) return;

        IsBusy = true;
        try
        {
            var result = await _apiService.GetAsync<List<TopicoForumDto>>("api/forum/topicos");
            Topicos.Clear();
            if (result != null)
            {
                foreach (var t in result) Topicos.Add(t);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading topicos: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }
}
