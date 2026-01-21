using Foundation;
using UIKit;

namespace zuoleme
{
    [Register("AppDelegate")]
    public class AppDelegate : MauiUIApplicationDelegate
    {
        protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

        public override bool FinishedLaunching(UIApplication application, NSDictionary? launchOptions)
        {
            // 设置 TabBar 样式
            UITabBar.Appearance.BackgroundColor = UIColor.SystemBackground;

            return base.FinishedLaunching(application, launchOptions);
        }
    }
}
