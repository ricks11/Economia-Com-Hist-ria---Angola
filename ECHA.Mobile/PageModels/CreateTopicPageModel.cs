using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EconomiaComHistoria.Core.DTOs;
using ECHA.Mobile.Services;

namespace ECHA.Mobile.PageModels;

public partial class CreateTopicPageModel : ObservableObject
{
    private readonly IApiService _apiService;

    [ObservableProperty]
    private string _titulo = string.Empty;

    [ObservableProperty]
    private string _descricao = string.Empty;

    [ObservableProperty]
    private int _categoriaId = 1; // Default category

    public CreateTopicPageModel(IApiService apiService)
    {
        _apiService = apiService;
    }

    [RelayCommand]
    private async Task CriarTopicoAsync()
    {
        try
        {
            var request = new CriarTopicoForumDto
            {
                Titulo = Titulo,
                Descricao = Descricao,
                CategoriaId = CategoriaId
            };
            await _apiService.PostAsync<CriarTopicoForumDto, TopicoForumDto>("api/forum/topicos", request);
            await Shell.Current.GoToAsync("..");
        }
        catch
        {
            await Shell.Current.DisplayAlert("Erro", "Falha ao criar tópico.", "OK");
        }
    }
}
