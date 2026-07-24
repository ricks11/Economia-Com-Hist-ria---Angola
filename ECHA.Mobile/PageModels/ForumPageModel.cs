using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EconomiaComHistoria.Core.DTOs;
using ECHA.Mobile.Services;
using System.Collections.ObjectModel;

namespace ECHA.Mobile.PageModels;

public partial class ForumTopicCard : ObservableObject
{
    public string Author { get; set; } = "";
    public string TimeAgo { get; set; } = "";
    public string Title { get; set; } = "";
    public string Summary { get; set; } = "";
    public string Comments { get; set; } = "0";
    public string Likes { get; set; } = "0";
    public string AvatarEmoji { get; set; } = "👤";
    public bool IsJindungo { get; set; }
    public int? TopicId { get; set; }
}

public partial class ForumPageModel : ObservableObject
{
    private readonly IApiService _apiService;

    [ObservableProperty]
    private bool _isBusy;

    public ObservableCollection<TopicoForumDto> Topicos { get; } = new();
    public ObservableCollection<ForumTopicCard> DisplayTopics { get; } = new();

    public ForumPageModel(IApiService apiService)
    {
        _apiService = apiService;
        SeedDesignTopics();
    }

    private void SeedDesignTopics()
    {
        DisplayTopics.Clear();
        DisplayTopics.Add(new ForumTopicCard
        {
            Author = "Dr. Agostinho Manuel",
            TimeAgo = "HÁ 2 HORAS",
            Title = "O impacto do café na era colonial e a formação de infraestruturas",
            Summary = "Como a monocultura do café moldou as rotas ferroviárias de Benguela e o porto de Luanda durante a década de 1950?",
            Comments = "42",
            Likes = "128",
            AvatarEmoji = "👨‍🏫"
        });
        DisplayTopics.Add(new ForumTopicCard
        {
            Author = "Isabel Santos",
            TimeAgo = "HÁ 5 HORAS",
            Title = "A transição económica de 1975: Desafios imediatos",
            Summary = "A saída súbita de capital humano e técnico no pós-independência e o início da economia centralizada.",
            Comments = "15",
            Likes = "56",
            AvatarEmoji = "👩‍💼"
        });
        DisplayTopics.Add(new ForumTopicCard
        {
            Author = "João Carlos",
            TimeAgo = "ONTEM",
            Title = "O Ciclo da Borracha no Planalto Central",
            Summary = "Análise sobre como o comércio da borracha no final do séc. XIX transformou as relações de poder locais.",
            Comments = "89",
            Likes = "312",
            AvatarEmoji = "🧑‍🎓",
            IsJindungo = true
        });
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
            if (result is { Count: > 0 })
            {
                foreach (var t in result) Topicos.Add(t);
                DisplayTopics.Clear();
                foreach (var t in result)
                {
                    DisplayTopics.Add(new ForumTopicCard
                    {
                        Author = t.AutorNome ?? "Curador",
                        TimeAgo = "RECENTE",
                        Title = t.Titulo ?? "",
                        Summary = t.Descricao ?? "",
                        Comments = "0",
                        Likes = "0",
                        TopicId = t.Id
                    });
                }
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

    [RelayCommand]
    private async Task CreateTopic()
    {
        await Shell.Current.GoToAsync("//CreateTopicPage");
    }

    [RelayCommand]
    private async Task OpenTopic(ForumTopicCard card)
    {
        await Shell.Current.GoToAsync("//TopicDetailPage");
    }
}
