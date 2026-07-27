using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace ECHA.Mobile.PageModels;

public partial class AchievementBadge : ObservableObject
{
    public string Emoji { get; set; } = "";
    public string Title { get; set; } = "";
    public string HowToEarn { get; set; } = "";
    public double Opacity { get; set; } = 1;
}

public partial class AchievementsPageModel : ObservableObject
{
    public ObservableCollection<AchievementBadge> Badges { get; } = new();

    public AchievementsPageModel()
    {
        Badges.Add(new AchievementBadge { Emoji = "🏦", Title = "Banco Nacional", HowToEarn = "Conclua o módulo 'Origens do Kwanza' com nota máxima." });
        Badges.Add(new AchievementBadge { Emoji = "📜", Title = "Arquivista Mor", HowToEarn = "Leia 50 artigos sobre a economia colonial." });
        Badges.Add(new AchievementBadge { Emoji = "🛢️", Title = "Barão do Petróleo", HowToEarn = "Ganhe o desafio 'Boom do Lobito'." });
        Badges.Add(new AchievementBadge { Emoji = "🔒", Title = "Reforma 1990", HowToEarn = "Desbloqueie o capítulo sobre a Transição Económica.", Opacity = 0.45 });
        Badges.Add(new AchievementBadge { Emoji = "☕", Title = "Rei do Café", HowToEarn = "Complete a simulação de exportação do Uíge." });
        Badges.Add(new AchievementBadge { Emoji = "💎", Title = "Lunda Norte", HowToEarn = "Explore todos os marcos geográficos de mineração." });
        Badges.Add(new AchievementBadge { Emoji = "🚢", Title = "Porto de Luanda", HowToEarn = "Complete o módulo de Comércio Exterior." });
        Badges.Add(new AchievementBadge { Emoji = "🚂", Title = "Caminho de Ferro", HowToEarn = "Estude a história da malha ferroviária nacional." });
        Badges.Add(new AchievementBadge { Emoji = "🏛️", Title = "Mestre Curador", HowToEarn = "Complete todos os desafios de trivia histórica.", Opacity = 0.45 });
    }

    [RelayCommand]
    private async Task GoBack()
    {
        await Shell.Current.GoToAsync("//ProfilePage");
    }

    [RelayCommand]
    private async Task ContinueLearning()
    {
        await Shell.Current.GoToAsync("//ExplorePage");
    }
}
