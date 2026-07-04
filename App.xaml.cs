using Microsoft.Extensions.DependencyInjection;
using zuoleme.Services;
using zuoleme.Views;

namespace zuoleme
{
    public partial class App : Application
    {
        private readonly IServiceProvider _serviceProvider;

        public App(IServiceProvider serviceProvider)
        {
            InitializeComponent();
            _serviceProvider = serviceProvider;

            // 初始化主题
            var themeService = serviceProvider.GetRequiredService<ThemeService>();
            themeService.InitializeTheme();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            var window = new Window(new AppShell());

            // 冷启动、以及从后台恢复时，如果开启了密码保护则显示锁屏
            window.Created += async (s, e) => await ShowLockScreenIfNeededAsync();
            window.Resumed += async (s, e) => await ShowLockScreenIfNeededAsync();

            return window;
        }

        private async Task ShowLockScreenIfNeededAsync()
        {
            try
            {
                var privacyService = _serviceProvider.GetRequiredService<PrivacyService>();
                if (!await privacyService.IsEnabledAsync()) return;

                if (Shell.Current?.Navigation == null) return;

                // 避免重复弹出锁屏
                if (Shell.Current.Navigation.ModalStack.Any(p => p is LockPage)) return;

                var lockPage = _serviceProvider.GetRequiredService<LockPage>();
                await Shell.Current.Navigation.PushModalAsync(lockPage, false);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"显示锁屏失败: {ex.Message}");
            }
        }
    }
}