using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using EconomiaComHistoria.Core.DTOs;
using EconomiaComHistoria.Core.Helpers;
using ECHA.Mobile.Services;

namespace ECHA.Mobile.PageModels
{
    public partial class MainPageModel : ObservableObject
    {
        private readonly IApiService _apiService;
        [ObservableProperty]
        bool _isBusy;

        [ObservableProperty]
        private string _contentStatus = "A carregar arquivos em destaque...";

        [ObservableProperty]
        private string _today = DateTime.Now.ToString("dddd, MMM d");

        [ObservableProperty]
        private string _welcomeMessage = "Bom dia, Curador.";

        [ObservableProperty]
        private string _subtitle = "Explore a evolução económica de Angola através de novos arquivos desenterrados hoje.";

        public ObservableCollection<FeaturedArchive> FeaturedArchives { get; } = new();

        public MainPageModel(IApiService apiService)
        {
            _apiService = apiService;
            WelcomeMessage = $"Bem-vindo ao, {Preferences.Default.Get("user_name", "Visitante")}";
        }

        private void LoadFeaturedArchives()
        {
            FeaturedArchives.Add(new FeaturedArchive
            {
                Title = "O Ciclo da Borracha (1890-1910)",
                Category = "Economia Colonial",
                Summary = "Uma análise detalhada das rotas comerciais e do impacto nas comunidades locais durante o boom da borracha.",
                ImageUrl = "https://lh3.googleusercontent.com/aida-public/AB6AXuAPlQnBiPuucPFyDkXKrVIPq-2sHgzQ0wk3dQCAR6dQFQdmPq2M7oZ1wniErgT4jMPAhKP99qBtX9xFbZTgmHocOwAuAT7VmmU5KObdA2oyIKd65vwI8Wafkc3JQ1XGW85px2IZbcyWNq_ZLApT2_Q7mrLQWsm9Y0DVSWHg_wlbSsjtdIVltAuW8TdHta1vlUC1ZL12LNMeLC1-zdU1D1rZCASDIfnkdP49CYTQXZhMfE9uwN3aCplCRiKbc3oV_loN-EZy71aCxdr",
                IsNew = true
            });
            
            FeaturedArchives.Add(new FeaturedArchive
            {
                Title = "Porto de Luanda: Uma Retrospectiva",
                Category = "Comércio Marítimo",
                Summary = "Como a infraestrutura portuária moldou as exportações no início do século XX.",
                ImageUrl = "https://lh3.googleusercontent.com/aida-public/AB6AXuD8FPOcc8S3UsDN6KDKmVj5gHp9dk2jYsW0-Mc_1yVzyU_4UCNU4wtKr6OeEoDFATTaTAdTgbMFgK7ojHNbfGeuX6wZK3-cqty-eqF5Tsw1heq2DqGkUuk9qigTvNSz8dfRqfchwKcuBkhdr7VUAnYDjZh0dqCFNyOLQc4wWfgBv6uszUmTx4HmAzpECTLCpkrbQQvtEiWtwwiy_9lmhjRQ0gxUpMOCzacm6ULjFoGjTOK7AJz7yE_1EP6a5xhIMPhSajLK83XWn7B",
                IsNew = false
            });
        }

        [RelayCommand]
        private async Task Appearing()
        {
            if (IsBusy) return;
            IsBusy = true;
            try
            {
                var result = await _apiService.GetAsync<PagedResult<ConteudoResponseDto>>("api/conteudos?pagina=1&tamanho=8");
                FeaturedArchives.Clear();
                foreach (var item in result?.Items ?? [])
                {
                    FeaturedArchives.Add(new FeaturedArchive
                    {
                        Title = item.Titulo,
                        Category = item.Tema,
                        Summary = item.Resumo ?? item.CorpoTexto ?? "Explorar este arquivo.",
                        ImageUrl = item.ThumbnailUrl ?? string.Empty,
                        IsNew = item.DataPublicacao >= DateTime.UtcNow.AddDays(-30)
                    });
                }
                ContentStatus = FeaturedArchives.Count == 0
                    ? "Ainda nao existem arquivos publicados."
                    : string.Empty;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                ContentStatus = "Nao foi possivel carregar os arquivos. Confirme que a API esta em execucao.";
            }
            finally { IsBusy = false; }
        }

        [RelayCommand]
        private async Task Refresh()
        {
            IsBusy = true;
            await Task.Delay(500); // Simulate load
            IsBusy = false;
        }
        
        [RelayCommand]
        private async Task NavigateToArchive(FeaturedArchive archive)
        {
            await Shell.Current.GoToAsync("//ExplorePage");
        }

        [RelayCommand]
        private async Task GoExplore()
        {
            await Shell.Current.GoToAsync("//ExplorePage");
        }

        [RelayCommand]
        private async Task GoNotifications()
        {
            await Shell.Current.GoToAsync("//NotificationsPage");
        }
    }

    public partial class FeaturedArchive : ObservableObject
    {
        [ObservableProperty]
        private string _title = string.Empty;
        
        [ObservableProperty]
        private string _category = string.Empty;
        
        [ObservableProperty]
        private string _summary = string.Empty;
        
        [ObservableProperty]
        private string _imageUrl = string.Empty;
        
        [ObservableProperty]
        private bool _isNew;
    }
}
