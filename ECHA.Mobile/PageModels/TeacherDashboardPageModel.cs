using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EconomiaComHistoria.Core.DTOs;
using ECHA.Mobile.Services;

namespace ECHA.Mobile.PageModels;

public partial class TeacherDashboardPageModel : ObservableObject
{
    private readonly IApiService _apiService;

    // ── Estatísticas de cabeçalho ──────────────────────────────────────────
    [ObservableProperty] private int _totalAlunos;
    [ObservableProperty] private int _totalTurmas;
    [ObservableProperty] private int _quizzesAtivos;
    [ObservableProperty] private string _mediaTurmaTexto = "–";

    // ── Turmas ────────────────────────────────────────────────────────────
    [ObservableProperty] private List<TurmaResumoDto> _turmas = new();

    // ── Alunos recentes ───────────────────────────────────────────────────
    [ObservableProperty] private List<AlunoAtividadeRecenteDto> _alunosRecentes = new();

    // ── Estado de carregamento ────────────────────────────────────────────
    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private bool _hasError;
    [ObservableProperty] private string _errorMessage = string.Empty;

    public TeacherDashboardPageModel(IApiService apiService)
    {
        _apiService = apiService;
    }

    [RelayCommand]
    private async Task LoadDashboardAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        HasError = false;

        try
        {
            var dashboard = await _apiService.GetAsync<ProfessorDashboardDto>("api/professor/dashboard");

            if (dashboard is null) return;

            TotalAlunos = dashboard.TotalAlunos;
            TotalTurmas = dashboard.TotalTurmas;
            QuizzesAtivos = dashboard.QuizzesAtivos;
            MediaTurmaTexto = dashboard.MediaPontosTurmas.ToString("N0") + " pts";
            Turmas = dashboard.Turmas ?? new();
            AlunosRecentes = dashboard.AlunosRecentes ?? new();
        }
        catch (Exception ex)
        {
            HasError = true;
            ErrorMessage = "Não foi possível carregar o painel. Verifique a sua ligação.";
            System.Diagnostics.Debug.WriteLine($"[TeacherDashboard] Erro: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task NavigateToRankingAsync()
    {
        await Shell.Current.GoToAsync("TurmaRankingPage");
    }

    [RelayCommand]
    private async Task NavigateToTurmaAsync(TurmaResumoDto turma)
    {
        await Shell.Current.GoToAsync($"TurmaRankingPage?turmaId={turma.Id}");
    }
}
