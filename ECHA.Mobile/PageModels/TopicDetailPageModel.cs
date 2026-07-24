using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EconomiaComHistoria.Core.DTOs;
using ECHA.Mobile.Services;
using System.Collections.ObjectModel;

namespace ECHA.Mobile.PageModels;

public partial class TopicReplyCard : ObservableObject
{
    public string Author { get; set; } = "";
    public string TimeAgo { get; set; } = "";
    public string Content { get; set; } = "";
}

public partial class TopicDetailPageModel : ObservableObject, IQueryAttributable
{
    private readonly IApiService _apiService;

    [ObservableProperty]
    private TopicoForumDto? _topico;

    [ObservableProperty]
    private List<RespostaForumDto>? _respostas;

    [ObservableProperty]
    private string _authorName = "Dr. Manuel dos Santos";

    [ObservableProperty]
    private string _authorMeta = "Curador de Economia • 2h atrás";

    [ObservableProperty]
    private string _topicTitle = "A Influência da Moeda Colonial na Estrutura Bancária de Luanda (1926-1950)";

    [ObservableProperty]
    private string _topicBody = "Como os ciclos do café e do diamante moldaram não apenas a balança comercial, mas a própria arquitetura física das instituições financeiras na Baixa de Luanda? Precisamos discutir como esses vestígios ainda informam a centralidade bancária atual.";

    [ObservableProperty]
    private string _commentCountLabel = "14 Comentários";

    [ObservableProperty]
    private bool _isPrivate = true;

    public ObservableCollection<TopicReplyCard> DisplayReplies { get; } = new();

    public TopicDetailPageModel(IApiService apiService)
    {
        _apiService = apiService;
        SeedDesignReplies();
    }

    private void SeedDesignReplies()
    {
        DisplayReplies.Clear();
        DisplayReplies.Add(new TopicReplyCard
        {
            Author = "Artur Mendes",
            TimeAgo = "1h atrás",
            Content = "É fascinante notar que muitos desses edifícios mantêm o estilo Art Déco, simbolizando a \"modernidade\" que a economia da época pretendia projetar para a metrópole."
        });
        DisplayReplies.Add(new TopicReplyCard
        {
            Author = "Isabel Chipenda",
            TimeAgo = "45min atrás",
            Content = "Exato, Artur. E se olharmos para a localização do antigo Banco de Angola, vemos como a economia era desenhada para o escoamento marítimo."
        });
        DisplayReplies.Add(new TopicReplyCard
        {
            Author = "Paulo Vunge",
            TimeAgo = "15min atrás",
            Content = "Algum de vocês tem dados sobre a circulação do Angolar comparado ao Escudo na região do Planalto Central nesse período?"
        });
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("Topico", out var topico))
        {
            Topico = (TopicoForumDto)topico;
            TopicTitle = Topico.Titulo ?? TopicTitle;
            TopicBody = Topico.Descricao ?? TopicBody;
            AuthorName = Topico.AutorNome ?? AuthorName;
            LoadRespostasCommand.Execute(null);
        }
    }

    [RelayCommand]
    private async Task LoadRespostasAsync()
    {
        if (Topico == null) return;
        var detalhe = await _apiService.GetAsync<TopicoForumDetalheDto>($"api/forum/topicos/{Topico.Id}");
        if (detalhe != null)
        {
            Respostas = detalhe.Respostas.ToList();
            CommentCountLabel = $"{detalhe.Respostas.Count} Comentários";
            DisplayReplies.Clear();
            foreach (var r in detalhe.Respostas)
            {
                DisplayReplies.Add(new TopicReplyCard
                {
                    Author = "Curador",
                    TimeAgo = "recente",
                    Content = r.Conteudo ?? ""
                });
            }
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

    [RelayCommand]
    private async Task GoBack()
    {
        await Shell.Current.GoToAsync("//ForumPage");
    }
}
