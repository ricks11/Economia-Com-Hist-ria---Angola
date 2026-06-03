using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ECHA.Mobile.Models;
using ECHA.Mobile.Services;
using System.Collections.ObjectModel;

namespace ECHA.Mobile.PageModels;

public partial class QuizListPageModel : ObservableObject
{
    private readonly IApiService _apiService;

    [ObservableProperty]
    private bool _isBusy;

    public ObservableCollection<QuizDto> Quizzes { get; } = new();

    public QuizListPageModel(IApiService apiService)
    {
        _apiService = apiService;
    }

    [RelayCommand]
    private async Task LoadQuizzesAsync()
    {
        if (IsBusy) return;

        IsBusy = true;
        try
        {
            var result = await _apiService.GetAsync<List<QuizDto>>("api/quizzes");
            Quizzes.Clear();
            if (result != null)
            {
                foreach (var q in result) Quizzes.Add(q);
            }
        }
        finally
        {
            IsBusy = false;
        }
    }
}
