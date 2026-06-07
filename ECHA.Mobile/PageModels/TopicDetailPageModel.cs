using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EconomiaComHistoria.Core.DTOs;
using ECHA.Mobile.Services;

namespace ECHA.Mobile.PageModels;

public partial class TopicDetailPageModel : ObservableObject, IQueryAttributable
{
    private readonly IApiService _apiService;

    [ObservableProperty]
    private TopicoForumDto? _topico;

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
            Topico = (TopicoForumDto)topico;
            LoadRespostasCommand.Execute(null);
        }
    }

    [RelayCommand]
    private async Task LoadRespostasAsync()
    {
        if (Topico == null) return;
        
        // The API returns TopicoForumDetalheDto for a specific topic, which includes answers.
        var detalhe = await _apiService.GetAsync<TopicoForumDetalheDto>($"api/forum/topicos/{Topico.Id}");
        if (detalhe != null)
        {
            Respostas = detalhe.Respostas.ToList();
        }
    }

    [RelayCommand]
    private async Task ReagirAsync(RespostaForumDto resposta)
    {
        await _apiService.PostAsync<object, object>("api/forum/reacoes", new { RespostaForumId = resposta.Id, Emoji = "👍" });
        await LoadRespostasCommand.ExecuteAsync(null);
    }

    [RelayCommand]
    private async Task DenunciarAsync(RespostaForumDto resposta)
    {
        await _apiService.PostAsync<object, object>("api/forum/denuncias", new { RespostaForumId = resposta.Id, Motivo = 0 });
        await Shell.Current.DisplayAlert("Denúncia", "Conteúdo denunciado.", "OK");
    }
}
