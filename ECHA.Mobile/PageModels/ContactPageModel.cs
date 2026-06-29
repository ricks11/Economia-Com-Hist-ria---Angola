using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ECHA.Mobile.PageModels;

public partial class ContactPageModel : ObservableObject
{
    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _message = string.Empty;

    public ContactPageModel()
    {
    }

    [RelayCommand]
    private async Task SendMessage()
    {
        // For now, just show an alert
        if (Application.Current?.MainPage != null)
        {
            await Application.Current.MainPage.DisplayAlert("Obrigado!", "Mensagem enviada com sucesso!", "OK");
        }
        Name = string.Empty;
        Message = string.Empty;
    }
}
