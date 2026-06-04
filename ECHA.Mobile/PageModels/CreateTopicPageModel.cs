using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ECHA.Mobile.Models;
using ECHA.Mobile.Services;

namespace ECHA.Mobile.PageModels;

public partial class CreateTopicPageModel : ObservableObject
{
    private readonly IApiService _apiService;

    [ObservableProperty]
    private string _titulo = string.Empty;

    [ObservableProperty]
    private string _descricao = string.Empty;

    public CreateTopicPageModel(IApiService apiService)
    {
        _apiService = apiService;
    }

    [RelayCommand]
    private async Task CriarTopicoAsync()
    {
        try
        {
            await _apiService.PostAsync<object, object>("api/forum/topicos", new { Titulo, Descricao });
            await Shell.Current.GoToAsync("..");
        }
        catch
        {
            await Shell.Current.DisplayAlert("Erro", "Falha ao criar tópico.", "OK");
        }
    }
}
