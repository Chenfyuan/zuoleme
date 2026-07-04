using zuoleme.Views;

namespace zuoleme
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            // 所有平台使用 TabBar，FlyoutBehavior 设为 Disabled
            FlyoutBehavior = FlyoutBehavior.Disabled;

            // 注册非 TabBar 的详情页路由
            Routing.RegisterRoute(nameof(InfoPage), typeof(InfoPage));
        }
    }
}
