namespace zuoleme
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            // 所有平台使用 TabBar，FlyoutBehavior 设为 Disabled
            FlyoutBehavior = FlyoutBehavior.Disabled;
        }
    }
}
