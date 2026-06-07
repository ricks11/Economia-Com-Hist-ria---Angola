using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EconomiaComHistoria.Core.DTOs;
using ECHA.Mobile.Services;
using System.Collections.ObjectModel;

namespace ECHA.Mobile.PageModels;

public partial class QuizListPageModel : ObservableObject
{
    private readonly IApiService _apiService;

    [ObservableProperty]
    private bool _isBusy;

    public ObservableCollection<QuizResponseDto> Quizzes { get; } = new();

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
            var result = await _apiService.GetAsync<List<QuizResponseDto>>("api/quizzes");
            Quizzes.Clear();
            if (result != null)
            {
                foreach (var q in result) Quizzes.Add(q);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading quizzes: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }
}
