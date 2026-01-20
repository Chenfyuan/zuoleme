using System.Text.Json;

namespace zuoleme.Services
{
    public class HealthService
    {
        private const string SETTINGS_FILE = "health_settings.json";
        private readonly string _settingsPath;

        public HealthService()
        {
            try
            {
                _settingsPath = Path.Combine(FileSystem.AppDataDirectory, SETTINGS_FILE);
            }
            catch
            {
                _settingsPath = SETTINGS_FILE;
            }
        }

        public class HealthSettings
        {
            public int Age { get; set; } = 25;
            public bool EnableHealthReminder { get; set; } = true;
        }

        public HealthSettings LoadSettings()
        {
            try
            {
                if (File.Exists(_settingsPath))
                {
                    var json = File.ReadAllText(_settingsPath);
                    var settings = JsonSerializer.Deserialize<HealthSettings>(json);
                    return settings ?? new HealthSettings();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"加载健康设置失败: {ex.Message}");
            }

            return new HealthSettings();
        }

        public void SaveSettings(HealthSettings settings)
        {
            try
            {
                var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_settingsPath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"保存健康设置失败: {ex.Message}");
            }
        }

        public int GetRecommendedWeeklyFrequency(int age)
        {
            try
            {
                if (age < 20) return 4;
                if (age < 30) return 3;
                if (age < 40) return 2;
                if (age < 50) return 2;
                return 1;
            }
            catch
            {
                return 3; // 默认值
            }
        }

        public HealthStatus GetHealthStatus(int weekCount, int recommendedCount)
        {
            try
            {
                if (recommendedCount <= 0)
                {
                    recommendedCount = 3; // 合理的默认值
                }

                var ratio = (double)weekCount / recommendedCount;

                if (ratio > 2.0)
                {
                    return new HealthStatus
                    {
                        Level = HealthLevel.TooHigh,
                        Title = "频率过高",
                        Message = "建议适当降低频率，注意休息",
                        Color = "#EF5350",
                        Icon = "\ue000", // MaterialIcons.Error - 错误图标
                        ProgressColor = "#EF5350",
                        BackgroundStartColor = "#FFEBEE",
                        BackgroundEndColor = "#FFCDD2",
                        ButtonColor = "#EF5350"
                    };
                }
                else if (ratio > 1.5)
                {
                    return new HealthStatus
                    {
                        Level = HealthLevel.High,
                        Title = "频率偏高",
                        Message = "可适当降低频率，保持健康",
                        Color = "#FF9800",
                        Icon = "\ue002", // MaterialIcons.Warning - 警告图标
                        ProgressColor = "#FF9800",
                        BackgroundStartColor = "#FFF3E0",
                        BackgroundEndColor = "#FFE0B2",
                        ButtonColor = "#FF9800"
                    };
                }
                else if (ratio >= 0.8)
                {
                    return new HealthStatus
                    {
                        Level = HealthLevel.Good,
                        Title = "频率良好",
                        Message = "保持当前的节奏，非常健康",
                        Color = "#4CAF50",
                        Icon = "\ue86c", // MaterialIcons.CheckCircle - 成功图标
                        ProgressColor = "#4CAF50",
                        BackgroundStartColor = "#E8F5E9",
                        BackgroundEndColor = "#C8E6C9",
                        ButtonColor = "#4CAF50"
                    };
                }
                else
                {
                    return new HealthStatus
                    {
                        Level = HealthLevel.Normal,
                        Title = "频率正常",
                        Message = "一切正常，继续保持！",
                        Color = "#2196F3",
                        Icon = "\ue88e", // MaterialIcons.Info - 信息图标
                        ProgressColor = "#2196F3",
                        BackgroundStartColor = "#E3F2FD",
                        BackgroundEndColor = "#BBDEFB",
                        ButtonColor = "#E91E63"
                    };
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"获取健康状态失败: {ex.Message}");
                return new HealthStatus
                {
                    Level = HealthLevel.Normal,
                    Title = "健康状态",
                    Message = "状态正常",
                    Color = "#2196F3",
                    Icon = "\ue88e", // MaterialIcons.Info - 信息图标
                    ProgressColor = "#2196F3",
                    BackgroundStartColor = "#E3F2FD",
                    BackgroundEndColor = "#BBDEFB",
                    ButtonColor = "#E91E63"
                };
            }
        }
    }

    public enum HealthLevel
    {
        Normal,
        Good,
        High,
        TooHigh
    }

    public class HealthStatus
    {
        public HealthLevel Level { get; set; }
        public string Title { get; set; } = "";
        public string Message { get; set; } = "";
        public string Color { get; set; } = "";
        public string Icon { get; set; } = "";
        public string ProgressColor { get; set; } = "";
        public string BackgroundStartColor { get; set; } = "";
        public string BackgroundEndColor { get; set; } = "";
        public string ButtonColor { get; set; } = "";
    }
}
