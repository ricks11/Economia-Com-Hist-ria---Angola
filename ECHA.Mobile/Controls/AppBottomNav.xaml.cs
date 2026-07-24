namespace ECHA.Mobile.Controls;

public partial class AppBottomNav : ContentView
{
    public static readonly BindableProperty ActiveTabProperty =
        BindableProperty.Create(nameof(ActiveTab), typeof(string), typeof(AppBottomNav), "inicio");

    public string ActiveTab
    {
        get => (string)GetValue(ActiveTabProperty);
        set => SetValue(ActiveTabProperty, value);
    }

    public AppBottomNav()
    {
        InitializeComponent();
    }

    private async void OnInicioTapped(object? sender, TappedEventArgs e)
        => await Shell.Current.GoToAsync("//MainPage");

    private async void OnAprenderTapped(object? sender, TappedEventArgs e)
        => await Shell.Current.GoToAsync("//ExplorePage");

    private async void OnJogarTapped(object? sender, TappedEventArgs e)
        => await Shell.Current.GoToAsync("//QuizListPage");

    private async void OnParticiparTapped(object? sender, TappedEventArgs e)
        => await Shell.Current.GoToAsync("//ForumPage");

    private async void OnPerfilTapped(object? sender, TappedEventArgs e)
        => await Shell.Current.GoToAsync("//ProfilePage");
}
