using System.Text.Json;
using Plugin.LocalNotification;
using Plugin.LocalNotification.Core.Models;

namespace zuoleme.Services
{
    /// <summary>
    /// 每日健康提醒通知服务
    /// </summary>
    public class ReminderService
    {
        private const string SettingsFile = "reminder_settings.json";
        private const int ReminderNotificationId = 9001;
        private readonly string _settingsPath;

        public ReminderService()
        {
            try
            {
                _settingsPath = Path.Combine(FileSystem.AppDataDirectory, SettingsFile);
            }
            catch
            {
                _settingsPath = SettingsFile;
            }
        }

        public class ReminderSettings
        {
            public bool IsEnabled { get; set; } = false;
            public int Hour { get; set; } = 21;
            public int Minute { get; set; } = 0;
        }

        public ReminderSettings LoadSettings()
        {
            try
            {
                if (File.Exists(_settingsPath))
                {
                    var json = File.ReadAllText(_settingsPath);
                    return JsonSerializer.Deserialize<ReminderSettings>(json) ?? new ReminderSettings();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"加载提醒设置失败: {ex.Message}");
            }

            return new ReminderSettings();
        }

        public void SaveSettings(ReminderSettings settings)
        {
            try
            {
                var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_settingsPath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"保存提醒设置失败: {ex.Message}");
            }
        }

        public async Task<bool> RequestPermissionAsync()
        {
            try
            {
                if (await LocalNotificationCenter.Current.AreNotificationsEnabled())
                {
                    return true;
                }

                return await LocalNotificationCenter.Current.RequestNotificationPermission();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"请求通知权限失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 根据当前设置重新调度每日提醒通知
        /// </summary>
        public async Task ApplyScheduleAsync(ReminderSettings settings)
        {
            try
            {
                LocalNotificationCenter.Current.Cancel(ReminderNotificationId);

                if (!settings.IsEnabled) return;

                var now = DateTimeOffset.Now;
                var notifyTime = new DateTimeOffset(now.Year, now.Month, now.Day, settings.Hour, settings.Minute, 0, now.Offset);
                if (notifyTime <= now)
                {
                    notifyTime = notifyTime.AddDays(1);
                }

                var request = new NotificationRequest
                {
                    NotificationId = ReminderNotificationId,
                    Title = "做了么",
                    Description = "健康小提醒，记得关注今天的状态～",
                    Schedule =
                    {
                        NotifyTime = notifyTime,
                        RepeatType = NotificationRepeat.Daily
                    }
                };

                await LocalNotificationCenter.Current.Show(request);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"设置提醒失败: {ex.Message}");
            }
        }
    }
}
