using Microsoft.Extensions.Logging;
using zuoleme.Services;
using zuoleme.ViewModels;
using zuoleme.Views;

namespace zuoleme
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddFont("MaterialSymbolsRounded.ttf", "MaterialSymbolsRounded");
                })
                .ConfigureMauiHandlers(handlers =>
                {
                    // 启用 Shell 的优化
#if WINDOWS
                    handlers.AddHandler<Shell, Microsoft.Maui.Controls.Handlers.ShellHandler>();
#endif
                });

            // 性能优化配置
            Microsoft.Maui.Handlers.LabelHandler.Mapper.AppendToMapping("PerformanceOptimization", (handler, view) =>
            {
                // 启用硬件加速
#if WINDOWS
                if (handler.PlatformView != null)
                {
                    handler.PlatformView.IsTextScaleFactorEnabled = true;
                }
#endif
            });

            // Register services
            builder.Services.AddSingleton<RecordService>();
            builder.Services.AddSingleton<HealthService>();
            builder.Services.AddSingleton<ThemeService>();

            // Register ViewModels
            builder.Services.AddSingleton<MainViewModel>();
            builder.Services.AddSingleton<CalendarViewModel>();
            builder.Services.AddSingleton<SettingsViewModel>();

            // Register pages - 使用 Transient 而不是 Singleton，让 Shell 缓存页面
            builder.Services.AddTransient<HomePage>();
            builder.Services.AddTransient<StatsPage>();
            builder.Services.AddTransient<CalendarPage>();
            builder.Services.AddTransient<SettingsPage>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
