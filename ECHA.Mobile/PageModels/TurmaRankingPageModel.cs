using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EconomiaComHistoria.Core.DTOs;
using ECHA.Mobile.Services;

namespace ECHA.Mobile.PageModels;

[QueryProperty(nameof(TurmaId), "turmaId")]
public partial class TurmaRankingPageModel : ObservableObject
{
    private readonly IApiService _apiService;

    [ObservableProperty]
    private int _turmaId;

    [ObservableProperty]
    private List<RankingEntradaDto> _ranking = new();

    [ObservableProperty]
    private TurmaRankingItemViewModel _firstPlace;

    [ObservableProperty]
    private TurmaRankingItemViewModel _secondPlace;

    [ObservableProperty]
    private TurmaRankingItemViewModel _thirdPlace;

    [ObservableProperty]
    private List<TurmaRankingItemViewModel> _displayRankings = new();

    [ObservableProperty]
    private int _currentUserPosicao = 5; // Mock logic

    public TurmaRankingPageModel(IApiService apiService)
    {
        _apiService = apiService;
    }

    [RelayCommand]
    private async Task LoadRankingAsync()
    {
        var response = await _apiService.GetAsync<RankingResponseDto>($"api/ranking/turma/{TurmaId}");
        var list = response?.Top100 ?? new();
        Ranking = list;

        var viewModels = list.Select(x => new TurmaRankingItemViewModel
        {
            Posicao = x.Posicao,
            NomeUtilizador = x.NomeUtilizador ?? "Utilizador",
            Pontos = x.Pontos,
            Initials = GetInitials(x.NomeUtilizador),
            // Mock logic for current user highlight (e.g. position 5)
            IsCurrentUser = x.Posicao == 5 
        }).ToList();

        FirstPlace = viewModels.FirstOrDefault(x => x.Posicao == 1);
        SecondPlace = viewModels.FirstOrDefault(x => x.Posicao == 2);
        ThirdPlace = viewModels.FirstOrDefault(x => x.Posicao == 3);

        DisplayRankings = viewModels.Where(x => x.Posicao > 3).ToList();
    }

    private string GetInitials(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return "??";
        var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 1) return parts[0].Substring(0, 1).ToUpper();
        return (parts[0].Substring(0, 1) + parts[^1].Substring(0, 1)).ToUpper();
    }

    [RelayCommand]
    private async Task GoBackAsync()
    {
        await Shell.Current.GoToAsync("..");
    }
}

public partial class TurmaRankingItemViewModel : ObservableObject
{
    [ObservableProperty]
    private int _posicao;
    [ObservableProperty]
    private string _nomeUtilizador;
    [ObservableProperty]
    private int _pontos;
    [ObservableProperty]
    private string _initials;
    [ObservableProperty]
    private bool _isCurrentUser;
}
