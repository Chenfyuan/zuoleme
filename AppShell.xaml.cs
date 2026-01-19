namespace zuoleme
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            
            // Shell 的 TabBar 在某些平台上原生支持滑动
            // 对于需要额外配置的平台，可以在这里添加
        }
    }
}
