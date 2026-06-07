using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using Syncfusion.Maui.Toolkit.Hosting;
using ECHA.Mobile.Services;
using Polly;
using Polly.Extensions.Http;

namespace ECHA.Mobile
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureSyncfusionToolkit()
                .ConfigureMauiHandlers(handlers =>
                {
#if IOS || MACCATALYST
    				handlers.AddHandler<Microsoft.Maui.Controls.CollectionView, Microsoft.Maui.Controls.Handlers.Items2.CollectionViewHandler2>();
#endif
                })
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddFont("SegoeUI-Semibold.ttf", "SegoeSemibold");
                    fonts.AddFont("FluentSystemIcons-Regular.ttf", FluentUI.FontFamily);
                });

#if DEBUG
    		builder.Logging.AddDebug();
    		builder.Services.AddLogging(configure => configure.AddDebug());
#endif

            builder.Services.AddSingleton<MainPageModel>();
            builder.Services.AddSingleton<MainPage>();
            builder.Services.AddSingleton<ExplorePageModel>();
            builder.Services.AddTransient<ExplorePage>();
            builder.Services.AddSingleton<ContentDetailPageModel>();
            builder.Services.AddTransient<ContentDetailPage>();
            builder.Services.AddSingleton<ProfilePageModel>();
            builder.Services.AddTransient<ProfilePage>();
            builder.Services.AddSingleton<QuizListPageModel>();
            builder.Services.AddTransient<QuizListPage>();
            builder.Services.AddSingleton<RankingPageModel>();
            builder.Services.AddTransient<RankingPage>();
            builder.Services.AddSingleton<ForumPageModel>();
            builder.Services.AddTransient<ForumPage>();
            builder.Services.AddSingleton<TopicDetailPageModel>();
            builder.Services.AddTransient<TopicDetailPage>();
            builder.Services.AddSingleton<CreateTopicPageModel>();
            builder.Services.AddTransient<CreateTopicPage>();
            builder.Services.AddSingleton<MapPageModel>();
            builder.Services.AddTransient<MapPage>();
            builder.Services.AddSingleton<StudyPlanPageModel>();
            builder.Services.AddTransient<StudyPlanPage>();
            builder.Services.AddSingleton<TurmaRankingPageModel>();
            builder.Services.AddTransient<TurmaRankingPage>();
            builder.Services.AddSingleton<TeacherDashboardPageModel>();
            builder.Services.AddTransient<TeacherDashboardPage>();
            
            builder.Services.AddSingleton<ApiService>();
            builder.Services.AddDbContext<CacheDbContext>();
            builder.Services.AddHttpClient<IApiService, ApiService>(client =>
            {
                client.BaseAddress = new Uri("http://localhost:5000/"); // Update with production URL later
            })
            .AddTransientHttpErrorPolicy(policy => policy.WaitAndRetryAsync(3, _ => TimeSpan.FromSeconds(2)));

            return builder.Build();
        }
    }
}
