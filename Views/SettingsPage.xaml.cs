using zuoleme.ViewModels;

namespace zuoleme.Views
{
    public partial class SettingsPage : ContentPage
    {
        private readonly SettingsViewModel _viewModel;

        public SettingsPage(SettingsViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
            _viewModel = viewModel;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            // 设置页面不需要频繁刷新
            System.Diagnostics.Debug.WriteLine("SettingsPage appeared");
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            System.Diagnostics.Debug.WriteLine("SettingsPage disappeared");
        }
    }
}
