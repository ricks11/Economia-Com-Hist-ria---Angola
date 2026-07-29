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
    private string _turmaNome = "Turma";

    [ObservableProperty]
    private TurmaRankingItemViewModel? _firstPlace;

    [ObservableProperty]
    private TurmaRankingItemViewModel? _secondPlace;

    [ObservableProperty]
    private TurmaRankingItemViewModel? _thirdPlace;

    [ObservableProperty]
    private List<TurmaRankingItemViewModel> _displayRankings = new();

    [ObservableProperty]
    private int _currentUserPosicao;

    [ObservableProperty]
    private bool _isBusy;

    public TurmaRankingPageModel(IApiService apiService)
    {
        _apiService = apiService;
    }

    [RelayCommand]
    private async Task LoadRankingAsync()
    {
        if (IsBusy) return;
        IsBusy = true;

        try
        {
            var endpoint = TurmaId > 0 ? $"api/turmas/{TurmaId}/ranking" : "api/ranking?tipo=Geral&periodo=Geral";

            if (TurmaId > 0)
            {
                var response = await _apiService.GetAsync<TurmaRankingResponseDto>(endpoint);

                if (response is not null)
                {
                    TurmaNome = response.TurmaNome ?? "Turma";
                    CurrentUserPosicao = response.PosicaoUtilizador;

                    var items = (response.Entradas ?? new()).Select(e => new TurmaRankingItemViewModel
                    {
                        Posicao = e.Posicao,
                        NomeUtilizador = e.NomeAluno ?? "Aluno",
                        Pontos = e.Pontos,
                        Initials = GetInitials(e.NomeAluno),
                        IsCurrentUser = e.IsCurrentUser
                    }).ToList();

                    FirstPlace = items.FirstOrDefault(x => x.Posicao == 1);
                    SecondPlace = items.FirstOrDefault(x => x.Posicao == 2);
                    ThirdPlace = items.FirstOrDefault(x => x.Posicao == 3);
                    DisplayRankings = items.Where(x => x.Posicao > 3).ToList();
                }
            }
            else
            {
                var response = await _apiService.GetAsync<RankingResponseDto>(endpoint);
                if (response?.Top100 is not null)
                {
                    CurrentUserPosicao = response.PosicaoUtilizador;
                    var items = response.Top100.Select(e => new TurmaRankingItemViewModel
                    {
                        Posicao = e.Posicao,
                        NomeUtilizador = e.NomeUtilizador ?? "Utilizador",
                        Pontos = e.Pontos,
                        Initials = GetInitials(e.NomeUtilizador),
                        IsCurrentUser = e.Posicao == response.PosicaoUtilizador
                    }).ToList();

                    FirstPlace = items.FirstOrDefault(x => x.Posicao == 1);
                    SecondPlace = items.FirstOrDefault(x => x.Posicao == 2);
                    ThirdPlace = items.FirstOrDefault(x => x.Posicao == 3);
                    DisplayRankings = items.Where(x => x.Posicao > 3).ToList();
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[TurmaRanking] Erro ao carregar: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private static string GetInitials(string? name)
    {
        if (string.IsNullOrWhiteSpace(name)) return "??";
        var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 1) return parts[0][..1].ToUpper();
        return (parts[0][..1] + parts[^1][..1]).ToUpper();
    }

    [RelayCommand]
    private async Task GoBackAsync()
    {
        await Shell.Current.GoToAsync("..");
    }
}

public partial class TurmaRankingItemViewModel : ObservableObject
{
    [ObservableProperty] private int _posicao;
    [ObservableProperty] private string _nomeUtilizador = string.Empty;
    [ObservableProperty] private int _pontos;
    [ObservableProperty] private string _initials = string.Empty;
    [ObservableProperty] private bool _isCurrentUser;
}
