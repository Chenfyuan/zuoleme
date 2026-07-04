using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using zuoleme.Services;
using zuoleme.Views;

namespace zuoleme.ViewModels
{
    public class SettingsViewModel : INotifyPropertyChanged
    {
        private readonly HealthService _healthService;
        private readonly RecordService _recordService;
        private readonly ThemeService _themeService;
        private readonly PrivacyService _privacyService;
        private readonly ReminderService _reminderService;
        private int _age = 25;
        private int _recommendedWeeklyCount = 3;
        private bool _isDarkMode = false;
        private bool _useSystemTheme = true;
        private bool _isPasswordProtectionEnabled = false;
        private bool _isReminderEnabled = false;
        private TimeSpan _reminderTime = new(21, 0, 0);

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

        public bool IsPasswordProtectionEnabled
        {
            get => _isPasswordProtectionEnabled;
            set
            {
                if (_isPasswordProtectionEnabled == value) return;
                _isPasswordProtectionEnabled = value;
                OnPropertyChanged();
                _ = HandlePasswordProtectionToggleAsync(value);
            }
        }

        public bool IsReminderEnabled
        {
            get => _isReminderEnabled;
            set
            {
                if (_isReminderEnabled == value) return;
                _isReminderEnabled = value;
                OnPropertyChanged();
                _ = HandleReminderToggleAsync(value);
            }
        }

        public TimeSpan ReminderTime
        {
            get => _reminderTime;
            set
            {
                if (_reminderTime == value) return;
                _reminderTime = value;
                OnPropertyChanged();
                if (IsReminderEnabled)
                {
                    SaveAndApplyReminder();
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
        public ICommand ExportDataCommand { get; }
        public ICommand ImportDataCommand { get; }
        public ICommand ShowHelpCommand { get; }
        public ICommand ShowChangelogCommand { get; }
        public ICommand ShowPrivacyPolicyCommand { get; }

        public SettingsViewModel(HealthService healthService, RecordService recordService, ThemeService themeService, PrivacyService privacyService, ReminderService reminderService)
        {
            _healthService = healthService;
            _recordService = recordService;
            _themeService = themeService;
            _privacyService = privacyService;
            _reminderService = reminderService;

            IncreaseAgeCommand = new Command(() => Age++);
            DecreaseAgeCommand = new Command(() => Age--);
            ClearDataCommand = new Command(async () => await ClearAllData());
            ExportDataCommand = new Command(async () => await ExportData());
            ImportDataCommand = new Command(async () => await ImportData());
            ShowHelpCommand = new Command(async () => await ShowInfoPageAsync("使用说明", HelpContent));
            ShowChangelogCommand = new Command(async () => await ShowInfoPageAsync("版本更新", ChangelogContent));
            ShowPrivacyPolicyCommand = new Command(async () => await ShowInfoPageAsync("隐私政策", PrivacyPolicyContent));

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

            LoadReminderSettings();
            _ = LoadPrivacySettingsAsync();
        }

        private void LoadReminderSettings()
        {
            try
            {
                var settings = _reminderService.LoadSettings();
                _isReminderEnabled = settings.IsEnabled;
                _reminderTime = new TimeSpan(settings.Hour, settings.Minute, 0);
                OnPropertyChanged(nameof(IsReminderEnabled));
                OnPropertyChanged(nameof(ReminderTime));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"加载提醒设置失败: {ex.Message}");
            }
        }

        private async Task HandleReminderToggleAsync(bool enable)
        {
            if (enable)
            {
                var granted = await _reminderService.RequestPermissionAsync();
                if (!granted)
                {
                    _isReminderEnabled = false;
                    OnPropertyChanged(nameof(IsReminderEnabled));
                    if (Application.Current?.MainPage != null)
                    {
                        await Application.Current.MainPage.DisplayAlert(
                            "提示", "未获得通知权限，无法开启提醒", "确定");
                    }
                    return;
                }
            }

            SaveAndApplyReminder();
        }

        private void SaveAndApplyReminder()
        {
            var settings = new ReminderService.ReminderSettings
            {
                IsEnabled = IsReminderEnabled,
                Hour = ReminderTime.Hours,
                Minute = ReminderTime.Minutes
            };

            _reminderService.SaveSettings(settings);
            _ = _reminderService.ApplyScheduleAsync(settings);
        }

        private async Task LoadPrivacySettingsAsync()
        {
            try
            {
                var enabled = await _privacyService.IsEnabledAsync();
                _isPasswordProtectionEnabled = enabled;
                OnPropertyChanged(nameof(IsPasswordProtectionEnabled));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"加载密码保护设置失败: {ex.Message}");
            }
        }

        private async Task HandlePasswordProtectionToggleAsync(bool enable)
        {
            if (Application.Current?.MainPage == null) return;

            if (enable)
            {
                var pin = await PromptForNewPinAsync();
                if (pin == null)
                {
                    RevertPasswordProtectionState(false);
                    return;
                }

                await _privacyService.SetPinAsync(pin);
            }
            else
            {
                var currentPin = await Application.Current.MainPage.DisplayPromptAsync(
                    "关闭密码保护", "请输入当前密码以确认", "确认", "取消",
                    keyboard: Keyboard.Numeric, maxLength: 6);

                if (string.IsNullOrEmpty(currentPin) || !await _privacyService.VerifyPinAsync(currentPin))
                {
                    if (currentPin != null)
                    {
                        await Application.Current.MainPage.DisplayAlert("错误", "密码不正确", "确定");
                    }
                    RevertPasswordProtectionState(true);
                    return;
                }

                await _privacyService.DisableAsync();
            }
        }

        private async Task<string?> PromptForNewPinAsync()
        {
            if (Application.Current?.MainPage == null) return null;

            var pin1 = await Application.Current.MainPage.DisplayPromptAsync(
                "设置密码", "请输入 4-6 位数字密码", "下一步", "取消",
                keyboard: Keyboard.Numeric, maxLength: 6);

            if (string.IsNullOrEmpty(pin1) || pin1.Length < 4 || !pin1.All(char.IsDigit))
            {
                if (pin1 != null)
                {
                    await Application.Current.MainPage.DisplayAlert("错误", "密码需为 4-6 位数字", "确定");
                }
                return null;
            }

            var pin2 = await Application.Current.MainPage.DisplayPromptAsync(
                "确认密码", "请再次输入密码", "确定", "取消",
                keyboard: Keyboard.Numeric, maxLength: 6);

            if (pin2 != pin1)
            {
                await Application.Current.MainPage.DisplayAlert("错误", "两次输入的密码不一致", "确定");
                return null;
            }

            return pin1;
        }

        private void RevertPasswordProtectionState(bool value)
        {
            _isPasswordProtectionEnabled = value;
            OnPropertyChanged(nameof(IsPasswordProtectionEnabled));
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

        private async Task ExportData()
        {
            try
            {
                var count = _recordService.GetRecordCount();
                if (count == 0)
                {
                    await Application.Current!.MainPage!.DisplayAlert("提示", "当前没有数据可导出", "确定");
                    return;
                }

                var json = _recordService.ExportRecordsToJson();
                var fileName = $"zuoleme_backup_{DateTime.Now:yyyyMMdd_HHmmss}.json";
                var filePath = Path.Combine(FileSystem.CacheDirectory, fileName);
                await File.WriteAllTextAsync(filePath, json);

                await Share.Default.RequestAsync(new ShareFileRequest
                {
                    Title = "导出做了么数据",
                    File = new ShareFile(filePath)
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"导出数据失败: {ex.Message}");
                await Application.Current!.MainPage!.DisplayAlert("错误", "导出数据失败，请重试", "确定");
            }
        }

        private async Task ImportData()
        {
            try
            {
                var pickResult = await FilePicker.Default.PickAsync(new PickOptions
                {
                    PickerTitle = "选择备份文件"
                });

                if (pickResult == null) return;

                var json = await File.ReadAllTextAsync(pickResult.FullPath);

                var action = await Application.Current!.MainPage!.DisplayActionSheet(
                    "选择导入方式", "取消", null, "合并到现有数据", "替换全部数据");

                if (string.IsNullOrEmpty(action) || action == "取消") return;

                bool merge = action == "合并到现有数据";

                if (!merge)
                {
                    var confirmReplace = await Application.Current.MainPage.DisplayAlert(
                        "确认替换",
                        "这将清空现有全部记录，并替换为导入的数据，此操作不可撤销！",
                        "确认替换",
                        "取消");
                    if (!confirmReplace) return;
                }

                var importedCount = _recordService.ImportRecordsFromJson(json, merge);

                await Application.Current.MainPage.DisplayAlert(
                    "导入完成",
                    importedCount > 0 ? $"成功导入 {importedCount} 条记录" : "没有可导入的新记录",
                    "确定");
            }
            catch (System.Text.Json.JsonException)
            {
                await Application.Current!.MainPage!.DisplayAlert("错误", "文件格式不正确，无法导入", "确定");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"导入数据失败: {ex.Message}");
                await Application.Current!.MainPage!.DisplayAlert("错误", "导入数据失败，请重试", "确定");
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

        private static async Task ShowInfoPageAsync(string title, string body)
        {
            if (Shell.Current == null) return;

            await Shell.Current.GoToAsync(nameof(InfoPage), new Dictionary<string, object>
            {
                { "title", title },
                { "body", body }
            });
        }

        private const string HelpContent =
            "欢迎使用「做了么」！\n\n" +
            "1. 主页点击中间的按钮即可一键记录。\n\n" +
            "2. 左滑记录卡片可以删除，点击记录卡片可以编辑备注。\n\n" +
            "3. 统计页可以查看今日/本周/本月/今年数据，以及最长连续天数、平均间隔天数等趋势图表。\n\n" +
            "4. 日历页点击某一天可以查看/编辑当天的记录备注。\n\n" +
            "5. 设置页可以调整年龄段以获取健康建议、导出/导入备份数据、开启密码保护和每日提醒、切换深色模式。\n\n" +
            "所有数据仅保存在本机，不会上传到任何服务器。";

        private const string ChangelogContent =
            "v1.1.0\n" +
            "· 新增记录备注功能，可在记录列表和日历中编辑\n" +
            "· 新增数据导出/导入（本地备份与恢复）\n" +
            "· 新增 PIN 密码保护与锁屏\n" +
            "· 新增每日健康提醒通知\n" +
            "· 统计页新增今年、最长连续天数、平均间隔天数\n" +
            "· 修复历史版本中的乱码显示问题\n\n" +
            "v1.0.0\n" +
            "· 首个版本发布：一键记录、统计图表可视化、日历视图、深色模式";

        private const string PrivacyPolicyContent =
            "「做了么」高度重视您的隐私：\n\n" +
            "1. 所有记录数据仅保存在您设备本地的应用沙盒内，不会上传至任何服务器或第三方。\n\n" +
            "2. 数据导出生成的备份文件由您自行选择分享方式和保存位置，应用本身不会读取或上传该文件。\n\n" +
            "3. 开启密码保护后，密码以哈希方式存储在系统安全存储中，应用不会以明文形式保存密码。\n\n" +
            "4. 每日提醒仅用于在本机弹出通知，不会收集或回传任何使用数据。\n\n" +
            "5. 卸载应用将删除本机保存的全部数据，请提前使用导出功能备份。";

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
