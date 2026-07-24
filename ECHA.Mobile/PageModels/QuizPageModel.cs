using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EconomiaComHistoria.Core.DTOs;
using System.Collections.ObjectModel;

namespace ECHA.Mobile.PageModels;

public partial class QuizOptionCard : ObservableObject
{
    public string Text { get; set; } = "";

    [ObservableProperty]
    private bool _isSelected;
}

public partial class QuizPageModel : ObservableObject, IQueryAttributable
{
    [ObservableProperty]
    private QuizStartResponseDto? _quizSession;

    [ObservableProperty]
    private int _currentQuestionIndex;

    [ObservableProperty]
    private double _progress = 0.4;

    [ObservableProperty]
    private int _secondsRemaining = 30;

    [ObservableProperty]
    private string _xpLabel = "⭐ 1,240 XP";

    [ObservableProperty]
    private string _moduleLabel = "MÓDULO 04: PERÍODO COLONIAL";

    [ObservableProperty]
    private string _progressLabel = "04/10";

    [ObservableProperty]
    private string _questionText = "Qual foi o principal produto de exportação de Angola durante o final do século XIX, antes da descoberta de petróleo em larga escala?";

    public ObservableCollection<QuizOptionCard> Options { get; } = new();

    public PerguntaStartDto? CurrentQuestion => QuizSession?.Perguntas[CurrentQuestionIndex];

    public QuizPageModel()
    {
        SeedDesignOptions();
    }

    private void SeedDesignOptions()
    {
        Options.Clear();
        Options.Add(new QuizOptionCard { Text = "Café e Borracha", IsSelected = true });
        Options.Add(new QuizOptionCard { Text = "Diamantes e Ouro" });
        Options.Add(new QuizOptionCard { Text = "Milho e Soja" });
        Options.Add(new QuizOptionCard { Text = "Algodão e Têxteis" });
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("QuizSession", out var session))
        {
            QuizSession = (QuizStartResponseDto)session;
            StartQuiz();
        }
    }

    private void StartQuiz()
    {
        CurrentQuestionIndex = 0;
        UpdateProgress();
        StartTimer();
        if (CurrentQuestion != null)
        {
            QuestionText = CurrentQuestion.Enunciado ?? QuestionText;
            Options.Clear();
            if (CurrentQuestion.Opcoes != null)
            {
                foreach (var o in CurrentQuestion.Opcoes)
                    Options.Add(new QuizOptionCard { Text = o.Texto ?? "" });
            }
        }
    }

    private void UpdateProgress()
    {
        if (QuizSession?.Perguntas == null) return;
        Progress = (double)(CurrentQuestionIndex + 1) / QuizSession.Perguntas.Count;
        ProgressLabel = $"{CurrentQuestionIndex + 1:00}/{QuizSession.Perguntas.Count:00}";
    }

    private void StartTimer()
    {
        IDispatcherTimer timer = Application.Current!.Dispatcher.CreateTimer();
        timer.Interval = TimeSpan.FromSeconds(1);
        timer.Tick += (s, e) =>
        {
            SecondsRemaining--;
            if (SecondsRemaining <= 0) timer.Stop();
        };
        timer.Start();
    }

    [RelayCommand]
    private void AnswerQuestion(OpcaoRespostaStartDto opcao) { }

    [RelayCommand]
    private void SelectOption(QuizOptionCard option)
    {
        foreach (var o in Options) o.IsSelected = false;
        option.IsSelected = true;
    }

    [RelayCommand]
    private void NextQuestion()
    {
        if (QuizSession?.Perguntas == null)
        {
            Progress = Math.Min(1, Progress + 0.1);
            return;
        }
        if (CurrentQuestionIndex < QuizSession.Perguntas.Count - 1)
        {
            CurrentQuestionIndex++;
            UpdateProgress();
            if (CurrentQuestion != null)
            {
                QuestionText = CurrentQuestion.Enunciado ?? QuestionText;
                Options.Clear();
                foreach (var o in CurrentQuestion.Opcoes ?? [])
                    Options.Add(new QuizOptionCard { Text = o.Texto ?? "" });
            }
        }
    }
}
