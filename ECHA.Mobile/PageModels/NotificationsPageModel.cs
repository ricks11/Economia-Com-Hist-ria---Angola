using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace ECHA.Mobile.PageModels;

public partial class NotificationCard : ObservableObject
{
    public string Icon { get; set; } = "";
    public string Title { get; set; } = "";
    public string Body { get; set; } = "";
    public string TimeAgo { get; set; } = "";
    public bool ShowActions { get; set; }
}

public partial class NotificationsPageModel : ObservableObject
{
    public ObservableCollection<NotificationCard> Notifications { get; } = new();

    public NotificationsPageModel()
    {
        Notifications.Add(new NotificationCard
        {
            Icon = "💬",
            Title = "João Santos",
            Body = "respondeu ao seu tópico: \"O Impacto do Café...\"",
            TimeAgo = "Há 2 horas"
        });
        Notifications.Add(new NotificationCard
        {
            Icon = "🔒",
            Title = "Dra. Elena Tavares",
            Body = "convidou-te para o fórum privado: \"Reformas de 1990\"",
            TimeAgo = "Há 5 horas",
            ShowActions = true
        });
        Notifications.Add(new NotificationCard
        {
            Icon = "📚",
            Title = "Recomendação:",
            Body = "O Ciclo do Ouro Negro - Um novo podcast disponível.",
            TimeAgo = "Ontem"
        });
        Notifications.Add(new NotificationCard
        {
            Icon = "✅",
            Title = "Seu artigo",
            Body = "\"Economia Informal\" foi aprovado e está agora publicado!",
            TimeAgo = "Ontem"
        });
        Notifications.Add(new NotificationCard
        {
            Icon = "🏅",
            Title = "Não percas o teu streak!",
            Body = "Completa o Quiz do dia sobre o Kwanza",
            TimeAgo = "Há 2 dias"
        });
    }

    [RelayCommand]
    private async Task GoBack()
    {
        await Shell.Current.GoToAsync("//MainPage");
    }
}
