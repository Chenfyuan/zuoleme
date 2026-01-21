using System.Text.Json;
using zuoleme.Models;
using zuoleme.Messages;
using CommunityToolkit.Mvvm.Messaging;

namespace zuoleme.Services
{
    public class RecordService
    {
        private readonly string _dataFilePath;
        private List<Record> _records;
        private readonly SemaphoreSlim _saveLock = new SemaphoreSlim(1, 1);
        private Task? _pendingSaveTask;

        public RecordService()
        {
            _dataFilePath = Path.Combine(FileSystem.AppDataDirectory, "records.json");
            _records = new List<Record>();
            LoadRecords();
        }

        public void AddRecord(Record record)
        {
            _records.Add(record);
            SaveRecordsAsync(); // 异步保存，不阻塞 UI
            SendDataChangedMessage(DataChangeType.RecordAdded);
        }

        public void DeleteRecord(Guid id)
        {
            var record = _records.FirstOrDefault(r => r.Id == id);
            if (record != null)
            {
                _records.Remove(record);
                SaveRecordsAsync(); // 异步保存
                SendDataChangedMessage(DataChangeType.RecordDeleted);
            }
        }

        public void ClearAllRecords()
        {
            try
            {
                _records.Clear();
                SaveRecordsAsync(); // 异步保存
                SendDataChangedMessage(DataChangeType.AllDataCleared);
                System.Diagnostics.Debug.WriteLine("所有记录已清空");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"清空记录失败: {ex.Message}");
                throw;
            }
        }

        public int GetRecordCount()
        {
            return _records.Count;
        }

        public List<Record> GetAllRecords()
        {
            // 返回副本，避免外部修改影响内部数据
            return _records.OrderByDescending(r => r.Timestamp).ToList();
        }

        public int GetTotalCount()
        {
            return _records.Count;
        }

        public int GetTodayCount()
        {
            var today = DateTime.Today;
            return _records.Count(r => r.Timestamp.Date == today);
        }

        public int GetWeekCount()
        {
            var startOfWeek = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek);
            return _records.Count(r => r.Timestamp.Date >= startOfWeek);
        }

        public int GetMonthCount()
        {
            var startOfMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            return _records.Count(r => r.Timestamp.Date >= startOfMonth);
        }

        /// <summary>
        /// 获取最近7天每天的记录数
        /// </summary>
        public Dictionary<DateTime, int> GetLast7DaysStats()
        {
            var result = new Dictionary<DateTime, int>();
            var today = DateTime.Today;

            for (int i = 6; i >= 0; i--)
            {
                var date = today.AddDays(-i);
                var count = _records.Count(r => r.Timestamp.Date == date);
                result[date] = count;
            }

            return result;
        }

        /// <summary>
        /// 获取最近6个月每月的记录数
        /// </summary>
        public Dictionary<(int Year, int Month), int> GetLast6MonthsStats()
        {
            var result = new Dictionary<(int Year, int Month), int>();
            var today = DateTime.Today;

            for (int i = 5; i >= 0; i--)
            {
                var date = today.AddMonths(-i);
                var key = (date.Year, date.Month);
                var count = _records.Count(r =>
                    r.Timestamp.Year == date.Year && r.Timestamp.Month == date.Month);
                result[key] = count;
            }

            return result;
        }

        /// <summary>
        /// 获取当月每天的记录数（用于热力图）
        /// </summary>
        public Dictionary<DateTime, int> GetCurrentMonthDailyStats()
        {
            var result = new Dictionary<DateTime, int>();
            var today = DateTime.Today;
            var startOfMonth = new DateTime(today.Year, today.Month, 1);
            var daysInMonth = DateTime.DaysInMonth(today.Year, today.Month);

            for (int day = 1; day <= daysInMonth; day++)
            {
                var date = new DateTime(today.Year, today.Month, day);
                var count = _records.Count(r => r.Timestamp.Date == date);
                result[date] = count;
            }

            return result;
        }

        /// <summary>
        /// 获取最近30天的每日记录数（用于折线图）
        /// </summary>
        public Dictionary<DateTime, int> GetLast30DaysStats()
        {
            var result = new Dictionary<DateTime, int>();
            var today = DateTime.Today;

            for (int i = 29; i >= 0; i--)
            {
                var date = today.AddDays(-i);
                var count = _records.Count(r => r.Timestamp.Date == date);
                result[date] = count;
            }

            return result;
        }

        private void LoadRecords()
        {
            try
            {
                if (File.Exists(_dataFilePath))
                {
                    var json = File.ReadAllText(_dataFilePath);
                    _records = JsonSerializer.Deserialize<List<Record>>(json) ?? new List<Record>();
                    System.Diagnostics.Debug.WriteLine($"加载了 {_records.Count} 条记录");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading records: {ex.Message}");
                _records = new List<Record>();
            }
        }

        private void SaveRecordsAsync()
        {
            // 如果有待处理的写入，取消它，重新开始
            _pendingSaveTask = Task.Run(async () =>
            {
                // 等待100ms，合并多次快速保存
                await Task.Delay(100);

                await _saveLock.WaitAsync();
                try
                {
                    var json = JsonSerializer.Serialize(_records, new JsonSerializerOptions
                    {
                        WriteIndented = false // 不格式化，减少文件大小
                    });

                    await File.WriteAllTextAsync(_dataFilePath, json);
                    System.Diagnostics.Debug.WriteLine($"保存了 {_records.Count} 条记录");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error saving records: {ex.Message}");
                }
                finally
                {
                    _saveLock.Release();
                }
            });
        }

        private void SendDataChangedMessage(DataChangeType changeType)
        {
            try
            {
                WeakReferenceMessenger.Default.Send(new DataChangedMessage(changeType));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"发送数据变更消息失败: {ex.Message}");
            }
        }
    }
}
