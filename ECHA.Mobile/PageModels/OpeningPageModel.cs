using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ECHA.Mobile.PageModels;

public partial class OpeningPageModel : ObservableObject
{
    [RelayCommand]
    private async Task EnterPortal()
    {
        if (Shell.Current != null)
            await Shell.Current.GoToAsync("//LandingPage");
    }

    [RelayCommand]
    private async Task GoExplore()
    {
        if (Shell.Current != null)
            await Shell.Current.GoToAsync("//ExplorePage");
    }

    [RelayCommand]
    private async Task GoQuiz()
    {
        if (Shell.Current != null)
            await Shell.Current.GoToAsync("//QuizListPage");
    }

    [RelayCommand]
    private async Task GoForum()
    {
        if (Shell.Current != null)
            await Shell.Current.GoToAsync("//ForumPage");
    }

    [RelayCommand]
    private async Task GoProfile()
    {
        if (Shell.Current != null)
            await Shell.Current.GoToAsync("//ProfilePage");
    }

    [RelayCommand]
    private async Task GoNotifications()
    {
        if (Shell.Current != null)
            await Shell.Current.GoToAsync("//NotificationsPage");
    }
}
