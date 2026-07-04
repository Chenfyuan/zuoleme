using zuoleme.ViewModels;

namespace zuoleme.Views
{
    public partial class LockPage : ContentPage
    {
        private readonly LockViewModel _viewModel;

        public LockPage(LockViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
            _viewModel = viewModel;

            _viewModel.Unlocked += OnUnlocked;
        }

        private void OnUnlocked()
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                try
                {
                    await Navigation.PopModalAsync(false);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"关闭锁屏失败: {ex.Message}");
                }
            });
        }

        protected override bool OnBackButtonPressed()
        {
            // 拦截 Android 物理返回键，防止绕过密码锁
            return true;
        }
    }
}
