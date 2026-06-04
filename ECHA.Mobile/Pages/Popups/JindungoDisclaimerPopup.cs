using CommunityToolkit.Maui.Views;

namespace ECHA.Mobile.Pages.Popups;

public class JindungoDisclaimerPopup : Popup
{
    public JindungoDisclaimerPopup()
    {
        Content = new VerticalStackLayout
        {
            Padding = 20,
            Spacing = 10,
            Children =
            {
                new Label { Text = "Aviso Editorial", FontSize = 20, FontAttributes = FontAttributes.Bold },
                new Label { Text = "Este conteúdo contém opinião crítica. O autor assume responsabilidade editorial.", FontSize = 16 },
                new Button { Text = "Entendido", Command = new Command(() => Close()) }
            }
        };
    }
}
