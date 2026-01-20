using System.Text.Json;

namespace zuoleme.Services
{
    public class ThemeService
    {
        private const string THEME_SETTINGS_FILE = "theme_settings.json";
        private readonly string _settingsPath;
        private bool _currentIsDarkMode = false;

        public ThemeService()
        {
            try
            {
                _settingsPath = Path.Combine(FileSystem.AppDataDirectory, THEME_SETTINGS_FILE);
            }
            catch
            {
                _settingsPath = THEME_SETTINGS_FILE;
            }
        }

        public class ThemeSettings
        {
            public bool IsDarkMode { get; set; } = false;
            public bool UseSystemTheme { get; set; } = true;
        }

        /// <summary>
        /// 加载主题设置
        /// </summary>
        public ThemeSettings LoadSettings()
        {
            try
            {
                if (File.Exists(_settingsPath))
                {
                    var json = File.ReadAllText(_settingsPath);
                    var settings = JsonSerializer.Deserialize<ThemeSettings>(json);
                    return settings ?? new ThemeSettings();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"加载主题设置失败: {ex.Message}");
            }

            return new ThemeSettings();
        }

        /// <summary>
        /// 保存主题设置
        /// </summary>
        public void SaveSettings(ThemeSettings settings)
        {
            try
            {
                var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_settingsPath, json);
                System.Diagnostics.Debug.WriteLine($"主题设置已保存: 深色模式={settings.IsDarkMode}, 跟随系统={settings.UseSystemTheme}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"保存主题设置失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 应用主题
        /// </summary>
        public void ApplyTheme(bool isDarkMode)
        {
            try
            {
                // 避免重复应用相同主题
                if (_currentIsDarkMode == isDarkMode)
                {
                    System.Diagnostics.Debug.WriteLine($"ℹ️ 主题未改变，跳过: {(isDarkMode ? "深色" : "浅色")}");
                    return;
                }

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    var app = Application.Current;
                    if (app == null) return;

                    try
                    {
                        // 加载对应的颜色资源字典
                        var themeFile = isDarkMode ? "ColorsDark.xaml" : "Colors.xaml";
                        var themeDictionary = new ResourceDictionary();
                        themeDictionary.Source = new Uri($"Resources/Styles/{themeFile}", UriKind.Relative);

                        System.Diagnostics.Debug.WriteLine($"🎨 加载主题文件: {themeFile}");

                        // 更新应用资源中的颜色
                        int updatedCount = 0;
                        foreach (var key in themeDictionary.Keys)
                        {
                            if (app.Resources.ContainsKey(key))
                            {
                                var oldValue = app.Resources[key];
                                var newValue = themeDictionary[key];
                                
                                // 只有值发生变化时才更新
                                if (!Equals(oldValue, newValue))
                                {
                                    app.Resources[key] = newValue;
                                    updatedCount++;
                                }
                            }
                            else
                            {
                                app.Resources.Add(key, themeDictionary[key]);
                                updatedCount++;
                            }
                        }

                        _currentIsDarkMode = isDarkMode;
                        System.Diagnostics.Debug.WriteLine($"✅ 主题已切换: {(isDarkMode ? "深色" : "浅色")} 模式 (更新了 {updatedCount} 个颜色)");
                        
                        // 发布主题变更事件
                        ThemeChanged?.Invoke(this, isDarkMode);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"❌ 应用主题时出错: {ex.Message}");
                        System.Diagnostics.Debug.WriteLine($"   Stack: {ex.StackTrace}");
                    }
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ 应用主题失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 应用系统主题设置
        /// </summary>
        public void ApplySystemTheme()
        {
            try
            {
                var systemTheme = Application.Current?.RequestedTheme ?? AppTheme.Light;
                var isDarkMode = systemTheme == AppTheme.Dark;
                System.Diagnostics.Debug.WriteLine($"📱 系统主题: {systemTheme} → {(isDarkMode ? "深色" : "浅色")}");
                ApplyTheme(isDarkMode);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"应用系统主题失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 初始化主题（应用启动时调用）
        /// </summary>
        public void InitializeTheme()
        {
            var settings = LoadSettings();
            
            System.Diagnostics.Debug.WriteLine($"🔧 初始化主题 - 跟随系统: {settings.UseSystemTheme}, 深色模式: {settings.IsDarkMode}");
            
            if (settings.UseSystemTheme)
            {
                ApplySystemTheme();
            }
            else
            {
                ApplyTheme(settings.IsDarkMode);
            }
        }

        /// <summary>
        /// 强制刷新主题（用于调试）
        /// </summary>
        public void ForceRefresh()
        {
            _currentIsDarkMode = !_currentIsDarkMode; // 强制改变状态
            ApplyTheme(!_currentIsDarkMode);
        }

        /// <summary>
        /// 主题变更事件
        /// </summary>
        public event EventHandler<bool>? ThemeChanged;
    }
}
