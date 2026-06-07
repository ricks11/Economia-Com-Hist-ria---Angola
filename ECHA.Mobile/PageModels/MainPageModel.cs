using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ECHA.Mobile.PageModels
{
    public partial class MainPageModel : ObservableObject
    {
        [ObservableProperty]
        bool _isBusy;

        [ObservableProperty]
        private string _today = DateTime.Now.ToString("dddd, MMM d");

        [ObservableProperty]
        private string _welcomeMessage = "Bem-vindo ao Economia com História - Angola";

        public MainPageModel()
        {
        }

        [RelayCommand]
        private async Task Appearing()
        {
            // Future dashboard logic here
            await Task.CompletedTask;
        }

        [RelayCommand]
        private async Task Refresh()
        {
            IsBusy = true;
            await Task.Delay(500); // Simulate load
            IsBusy = false;
        }
    }
}