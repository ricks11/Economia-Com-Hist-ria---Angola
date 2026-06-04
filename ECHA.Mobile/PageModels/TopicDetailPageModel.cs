using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ECHA.Mobile.Models;
using ECHA.Mobile.Services;

namespace ECHA.Mobile.PageModels;

public partial class TopicDetailPageModel : ObservableObject, IQueryAttributable
{
    private readonly IApiService _apiService;

    [ObservableProperty]
    private TopicoDto? _topico;

    [ObservableProperty]
    private List<RespostaForumDto>? _respostas;

    public TopicDetailPageModel(IApiService apiService)
    {
        _apiService = apiService;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("Topico", out var topico))
        {
            Topico = (TopicoDto)topico;
            LoadRespostasCommand.Execute(null);
        }
    }

    [RelayCommand]
    private async Task LoadRespostasAsync()
    {
        if (Topico == null) return;
        Respostas = await _apiService.GetAsync<List<RespostaForumDto>>($"api/forum/topicos/{Topico.Id}/respostas");
    }

    [RelayCommand]
    private async Task ReagirAsync(RespostaForumDto resposta)
    {
        await _apiService.PostAsync<object, object>($"api/forum/respostas/{resposta.Id}/reagir", new { });
        await LoadRespostasCommand.ExecuteAsync(null);
    }

    [RelayCommand]
    private async Task DenunciarAsync(RespostaForumDto resposta)
    {
        await _apiService.PostAsync<object, object>($"api/forum/respostas/{resposta.Id}/denunciar", new { });
        await Shell.Current.DisplayAlert("Denúncia", "Conteúdo denunciado.", "OK");
    }
}
