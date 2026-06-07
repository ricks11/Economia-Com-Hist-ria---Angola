using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EconomiaComHistoria.Core.DTOs;

namespace ECHA.Mobile.PageModels;

public partial class QuizPageModel : ObservableObject, IQueryAttributable
{
    [ObservableProperty]
    private QuizStartResponseDto? _quizSession;

    [ObservableProperty]
    private int _currentQuestionIndex;

    [ObservableProperty]
    private double _progress;

    [ObservableProperty]
    private int _secondsRemaining = 30;

    public PerguntaStartDto? CurrentQuestion => QuizSession?.Perguntas[CurrentQuestionIndex];

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
    }

    private void UpdateProgress()
    {
        if (QuizSession?.Perguntas == null) return;
        Progress = (double)(CurrentQuestionIndex + 1) / QuizSession.Perguntas.Count;
    }

    private void StartTimer()
    {
        // Simple timer implementation
        IDispatcherTimer timer = Application.Current!.Dispatcher.CreateTimer();
        timer.Interval = TimeSpan.FromSeconds(1);
        timer.Tick += (s, e) =>
        {
            SecondsRemaining--;
            if (SecondsRemaining <= 0)
            {
                timer.Stop();
                // Handle timeout
            }
        };
        timer.Start();
    }

    [RelayCommand]
    private void AnswerQuestion(OpcaoRespostaStartDto opcao)
    {
        // Feedback and next question logic
    }
}
