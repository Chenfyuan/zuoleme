using Microsoft.Extensions.DependencyInjection;
using zuoleme.Services;

namespace zuoleme
{
    public partial class App : Application
    {
        public App(IServiceProvider serviceProvider)
        {
            InitializeComponent();
            
            // 初始化主题
            var themeService = serviceProvider.GetRequiredService<ThemeService>();
            themeService.InitializeTheme();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new AppShell());
        }
    }
}