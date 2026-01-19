using zuoleme.ViewModels;

namespace zuoleme.Views
{
    public partial class StatsPage : ContentPage
    {
        private readonly MainViewModel _viewModel;

        public StatsPage(MainViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
            _viewModel = viewModel;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            // 不需要重新加载数据，ViewModel 通过消息机制自动同步
            System.Diagnostics.Debug.WriteLine("StatsPage appeared - 使用缓存数据");
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            System.Diagnostics.Debug.WriteLine("StatsPage disappeared");
        }
    }
}
