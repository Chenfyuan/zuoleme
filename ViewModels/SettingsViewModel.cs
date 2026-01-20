using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using zuoleme.Services;

namespace zuoleme.ViewModels
{
    public class SettingsViewModel : INotifyPropertyChanged
    {
        private readonly HealthService _healthService;
        private readonly RecordService _recordService;
        private readonly ThemeService _themeService;
        private int _age = 25;
        private int _recommendedWeeklyCount = 3;
        private bool _isDarkMode = false;
        private bool _useSystemTheme = true;

        public int Age
        {
            get => _age;
            set
            {
                if (value > 0 && value < 150 && _age != value)
                {
                    _age = value;
                    OnPropertyChanged();
                    UpdateRecommendedCount();
                    SaveSettings();
                }
            }
        }

        public int RecommendedWeeklyCount
        {
            get => _recommendedWeeklyCount;
            set
            {
                _recommendedWeeklyCount = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(RecommendedText));
                OnPropertyChanged(nameof(AgeRangeText));
            }
        }

        public bool IsDarkMode
        {
            get => _isDarkMode;
            set
            {
                if (_isDarkMode != value)
                {
                    _isDarkMode = value;
                    OnPropertyChanged();
                    
                    // 不跟随系统主题时，应用主题
                    if (!UseSystemTheme)
                    {
                        SaveThemeSettings();
                        _themeService.ApplyTheme(value);
                    }
                }
            }
        }

        public bool UseSystemTheme
        {
            get => _useSystemTheme;
            set
            {
                if (_useSystemTheme != value)
                {
                    _useSystemTheme = value;
                    OnPropertyChanged();
                    SaveThemeSettings();
                    
                    if (value)
                    {
                        // 跟随系统主题
                        _themeService.ApplySystemTheme();
                    }
                    else
                    {
                        // 使用手动设置的主题
                        _themeService.ApplyTheme(IsDarkMode);
                    }
                }
            }
        }

        public string RecommendedText => $"每周建议 {RecommendedWeeklyCount} 次";

        public string AgeRangeText
        {
            get
            {
                if (Age < 20) return "20岁以下";
                if (Age < 30) return "20-30岁";
                if (Age < 40) return "30-40岁";
                if (Age < 50) return "40-50岁";
                return "50岁以上";
            }
        }

        public ICommand IncreaseAgeCommand { get; }
        public ICommand DecreaseAgeCommand { get; }
        public ICommand ClearDataCommand { get; }

        public SettingsViewModel(HealthService healthService, RecordService recordService, ThemeService themeService)
        {
            _healthService = healthService;
            _recordService = recordService;
            _themeService = themeService;
            
            IncreaseAgeCommand = new Command(() => Age++);
            DecreaseAgeCommand = new Command(() => Age--);
            ClearDataCommand = new Command(async () => await ClearAllData());

            try
            {
                LoadSettings();
                LoadThemeSettings();
            }
            catch
            {
                // 使用默认值
                Age = 25;
                UpdateRecommendedCount();
            }
        }

        private async Task ClearAllData()
        {
            try
            {
                var recordCount = _recordService.GetRecordCount();
                
                if (recordCount == 0)
                {
                    await Application.Current!.MainPage!.DisplayAlert(
                        "提示", 
                        "当前没有数据需要清空", 
                        "确定");
                    return;
                }

                bool confirm = await Application.Current!.MainPage!.DisplayAlert(
                    "确认清空数据",
                    $"确定要清空所有记录吗？\n\n当前共有 {recordCount} 条记录\n此操作不可撤销！",
                    "清空",
                    "取消");

                if (confirm)
                {
                    _recordService.ClearAllRecords();
                    
                    await Application.Current.MainPage.DisplayAlert(
                        "成功",
                        "所有记录已清空",
                        "确定");
                    
                    // 清空后MessagingCenter会自动通知MainViewModel刷新
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"清空数据失败: {ex.Message}");
                
                await Application.Current!.MainPage!.DisplayAlert(
                    "错误",
                    "清空数据失败，请重试",
                    "确定");
            }
        }

        private void LoadSettings()
        {
            var settings = _healthService.LoadSettings();
            _age = settings.Age > 0 && settings.Age < 150 ? settings.Age : 25;
            OnPropertyChanged(nameof(Age));
            UpdateRecommendedCount();
        }

        private void SaveSettings()
        {
            try
            {
                var settings = new HealthService.HealthSettings
                {
                    Age = Age
                };
                _healthService.SaveSettings(settings);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"保存设置失败: {ex.Message}");
            }
        }

        private void LoadThemeSettings()
        {
            try
            {
                var settings = _themeService.LoadSettings();
                _useSystemTheme = settings.UseSystemTheme;
                _isDarkMode = settings.IsDarkMode;
                
                OnPropertyChanged(nameof(UseSystemTheme));
                OnPropertyChanged(nameof(IsDarkMode));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"加载主题设置失败: {ex.Message}");
            }
        }

        private void SaveThemeSettings()
        {
            try
            {
                var settings = new ThemeService.ThemeSettings
                {
                    IsDarkMode = IsDarkMode,
                    UseSystemTheme = UseSystemTheme
                };
                _themeService.SaveSettings(settings);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"保存主题设置失败: {ex.Message}");
            }
        }

        private void UpdateRecommendedCount()
        {
            try
            {
                RecommendedWeeklyCount = _healthService.GetRecommendedWeeklyFrequency(Age);
            }
            catch
            {
                RecommendedWeeklyCount = 3; // 默认值
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
