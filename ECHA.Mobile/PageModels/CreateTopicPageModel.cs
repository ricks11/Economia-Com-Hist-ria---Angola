using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EconomiaComHistoria.Core.DTOs;
using ECHA.Mobile.Services;
using System.Collections.ObjectModel;

namespace ECHA.Mobile.PageModels;

public partial class CreateTopicPageModel : ObservableObject
{
    private readonly IApiService _apiService;

    [ObservableProperty]
    private string _titulo = string.Empty;

    [ObservableProperty]
    private string _descricao = string.Empty;

    [ObservableProperty]
    private int _categoriaId = 1;

    [ObservableProperty]
    private bool _isPublic = true;

    [ObservableProperty]
    private string _invitees = string.Empty;

    [ObservableProperty]
    private bool _allowComments = true;

    [ObservableProperty]
    private string? _selectedCategoria;

    public ObservableCollection<string> Categorias { get; } =
    [
        "Economia de Lunda",
        "História Pré-Colonial",
        "Arte e Artefactos",
        "Debates Contemporâneos",
        "História Económica",
        "Recursos Naturais"
    ];

    public CreateTopicPageModel(IApiService apiService)
    {
        _apiService = apiService;
        SelectedCategoria = Categorias[0];
    }

    [RelayCommand]
    private void SetPublic() => IsPublic = true;

    [RelayCommand]
    private void SetPrivate() => IsPublic = false;

    [RelayCommand]
    private async Task CriarTopicoAsync()
    {
        try
        {
            var request = new CriarTopicoForumDto(Titulo, Descricao, CategoriaId);
            await _apiService.PostAsync<CriarTopicoForumDto, TopicoForumDto>("api/forum/topicos", request);
            await Shell.Current.GoToAsync("//ForumPage");
        }
        catch
        {
            await Shell.Current.DisplayAlert("Tópico", "Pré-visualização pronta. A API será ligada depois.", "OK");
            await Shell.Current.GoToAsync("//ForumPage");
        }
    }
}
