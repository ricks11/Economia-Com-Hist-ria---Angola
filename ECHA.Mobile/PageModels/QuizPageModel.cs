using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ECHA.Mobile.Models;

namespace ECHA.Mobile.PageModels;

public partial class QuizPageModel : ObservableObject, IQueryAttributable
{
    [ObservableProperty]
    private QuizDto? _quiz;

    [ObservableProperty]
    private int _currentQuestionIndex;

    [ObservableProperty]
    private double _progress;

    [ObservableProperty]
    private int _secondsRemaining = 30;

    public PerguntaDto CurrentQuestion => Quiz!.Perguntas[CurrentQuestionIndex];

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("Quiz", out var quiz))
        {
            Quiz = (QuizDto)quiz;
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
        Progress = (double)(CurrentQuestionIndex + 1) / Quiz!.Perguntas.Count;
    }

    private void StartTimer()
    {
        // Simple timer implementation
        Device.StartTimer(TimeSpan.FromSeconds(1), () =>
        {
            SecondsRemaining--;
            if (SecondsRemaining <= 0)
            {
                // Handle timeout
                return false;
            }
            return true;
        });
    }

    [RelayCommand]
    private void AnswerQuestion(RespostaDto resposta)
    {
        // Feedback and next question logic
    }
}
