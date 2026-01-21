using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using zuoleme.Models;
using zuoleme.Services;
using zuoleme.Messages;
using CommunityToolkit.Mvvm.Messaging;

namespace zuoleme.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly RecordService _recordService;
        private readonly HealthService _healthService;
        private bool _isLoading = false;
        private DateTime _lastRefreshTime = DateTime.MinValue;
        private const int REFRESH_THROTTLE_MS = 500; // 防抖：500ms内只刷新一次

        private int _totalCount;
        private int _todayCount;
        private int _weekCount;
        private int _monthCount;
        private int _age;
        private int _recommendedWeeklyCount = 3;
        private HealthStatus? _healthStatus;
        private double _weekProgress;
        private string _greetingText = "";
        private string _statusTipText = "";

        public int TotalCount
        {
            get => _totalCount;
            set
            {
                _totalCount = value;
                OnPropertyChanged();
            }
        }

        public int TodayCount
        {
            get => _todayCount;
            set
            {
                _todayCount = value;
                OnPropertyChanged();
                UpdateStatusTip();
            }
        }

        public int WeekCount
        {
            get => _weekCount;
            set
            {
                _weekCount = value;
                OnPropertyChanged();
                UpdateHealthStatus();
            }
        }

        public int MonthCount
        {
            get => _monthCount;
            set
            {
                _monthCount = value;
                OnPropertyChanged();
            }
        }

        public int Age
        {
            get => _age;
            set
            {
                _age = value;
                OnPropertyChanged();
                UpdateRecommendedCount();
            }
        }

        public int RecommendedWeeklyCount
        {
            get => _recommendedWeeklyCount;
            set
            {
                if (value > 0)
                {
                    _recommendedWeeklyCount = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(RecommendedText));
                    UpdateHealthStatus();
                }
            }
        }

        public string RecommendedText => $"建议：每周 {RecommendedWeeklyCount} 次";

        public HealthStatus? HealthStatus
        {
            get => _healthStatus;
            set
            {
                _healthStatus = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HealthStatusTitle));
                OnPropertyChanged(nameof(HealthStatusMessage));
                OnPropertyChanged(nameof(HealthStatusColor));
                OnPropertyChanged(nameof(HealthStatusIcon));
                OnPropertyChanged(nameof(HealthStatusIconImage));
                OnPropertyChanged(nameof(HealthProgressColor));
                OnPropertyChanged(nameof(HealthBackgroundStartColor));
                OnPropertyChanged(nameof(HealthBackgroundEndColor));
                OnPropertyChanged(nameof(ButtonBackgroundColor));
            }
        }

        public string HealthStatusTitle => HealthStatus?.Title ?? "健康状态";
        public string HealthStatusMessage => HealthStatus?.Message ?? "状态正常";
        public string HealthStatusColor => HealthStatus?.Color ?? "#2196F3";
        public string HealthStatusIcon => HealthStatus?.Icon ?? "info.png";
        public string HealthStatusIconImage => HealthStatus?.Icon ?? "info.png";
        public string HealthProgressColor => HealthStatus?.ProgressColor ?? "#2196F3";
        public string HealthBackgroundStartColor => HealthStatus?.BackgroundStartColor ?? "#E3F2FD";
        public string HealthBackgroundEndColor => HealthStatus?.BackgroundEndColor ?? "#BBDEFB";
        public string ButtonBackgroundColor => HealthStatus?.ButtonColor ?? "#E91E63";

        public double WeekProgress
        {
            get => _weekProgress;
            set
            {
                _weekProgress = Math.Max(0, Math.Min(1, value));
                OnPropertyChanged();
                OnPropertyChanged(nameof(WeekProgressText));
            }
        }

        public string WeekProgressText => $"{WeekCount}/{RecommendedWeeklyCount}";

        public string GreetingText
        {
            get => _greetingText;
            set
            {
                _greetingText = value;
                OnPropertyChanged();
            }
        }

        public string StatusTipText
        {
            get => _statusTipText;
            set
            {
                _statusTipText = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<Record> Records { get; set; }
        public ObservableCollection<Record> RecentRecords { get; set; }
        public ObservableCollection<DailyStats> WeeklyChartData { get; set; }
        public ObservableCollection<MonthlyStats> MonthlyChartData { get; set; }
        public ObservableCollection<HeatmapCell> HeatmapData { get; set; }
        public ObservableCollection<LineChartPoint> LineChartData { get; set; }

        private string _currentMonthLabel = "";
        public string CurrentMonthLabel
        {
            get => _currentMonthLabel;
            set
            {
                _currentMonthLabel = value;
                OnPropertyChanged();
            }
        }

        public ICommand AddRecordCommand { get; }
        public ICommand DeleteRecordCommand { get; }
        public ICommand RefreshCommand { get; }

        public MainViewModel(RecordService recordService, HealthService healthService)
        {
            _recordService = recordService;
            _healthService = healthService;
            Records = new ObservableCollection<Record>();
            RecentRecords = new ObservableCollection<Record>();
            WeeklyChartData = new ObservableCollection<DailyStats>();
            MonthlyChartData = new ObservableCollection<MonthlyStats>();
            HeatmapData = new ObservableCollection<HeatmapCell>();
            LineChartData = new ObservableCollection<LineChartPoint>();

            AddRecordCommand = new Command(AddRecord);
            DeleteRecordCommand = new Command<Guid>(DeleteRecord);
            RefreshCommand = new Command(LoadData);

            try
            {
                LoadHealthSettings();
            }
            catch
            {
                Age = 25;
            }

            LoadData();
            UpdateGreetingAndTip();

            // 订阅数据变更消息
            SubscribeToDataChanges();
        }

        private void SubscribeToDataChanges()
        {
            WeakReferenceMessenger.Default.Register<DataChangedMessage>(this, (recipient, message) =>
            {
                System.Diagnostics.Debug.WriteLine($"收到数据变更消息: {message.ChangeType}");

                // 防抖刷新，短时间内多次刷新
                var timeSinceLastRefresh = (DateTime.Now - _lastRefreshTime).TotalMilliseconds;
                if (timeSinceLastRefresh < REFRESH_THROTTLE_MS && message.ChangeType != DataChangeType.AllDataCleared)
                {
                    System.Diagnostics.Debug.WriteLine($"防抖：距离上次刷新仅 {timeSinceLastRefresh}ms，跳过");
                    return;
                }

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    LoadData();
                });
            });
        }

        private void LoadHealthSettings()
        {
            var settings = _healthService.LoadSettings();
            Age = settings.Age > 0 ? settings.Age : 25;
        }

        private void UpdateGreetingAndTip()
        {
            UpdateGreeting();
            UpdateStatusTip();
        }

        private void UpdateGreeting()
        {
            var hour = DateTime.Now.Hour;
            string timeGreeting;
            string emoji;

            if (hour >= 0 && hour < 6)
            {
                timeGreeting = "凌晨好";
                emoji = "🌙";
            }
            else if (hour >= 6 && hour < 9)
            {
                timeGreeting = "早上好";
                emoji = "🌅";
            }
            else if (hour >= 9 && hour < 12)
            {
                timeGreeting = "上午好";
                emoji = "☀️";
            }
            else if (hour >= 12 && hour < 14)
            {
                timeGreeting = "中午好";
                emoji = "🌞";
            }
            else if (hour >= 14 && hour < 18)
            {
                timeGreeting = "下午好";
                emoji = "🌤️";
            }
            else if (hour >= 18 && hour < 22)
            {
                timeGreeting = "晚上好";
                emoji = "🌆";
            }
            else
            {
                timeGreeting = "深夜好";
                emoji = "🌃";
            }

            GreetingText = $"{timeGreeting}！老弟 {emoji}";
        }

        private void UpdateStatusTip()
        {
            if (TodayCount == 0)
            {
                StatusTipText = "今天还没做，快来一发吧";
            }
            else if (TodayCount == 3)
            {
                StatusTipText = "一天三次郎？你是认真的吗？";
            }
            else if (TodayCount > 3)
            {
                StatusTipText = "兄弟，你这是要逆天啊！";
            }
            else if (TodayCount == 1)
            {
                StatusTipText = "不错哦，保持健康节奏";
            }
            else if (TodayCount == 2)
            {
                StatusTipText = "今天很给力呀！";
            }
        }

        private void UpdateRecommendedCount()
        {
            RecommendedWeeklyCount = _healthService.GetRecommendedWeeklyFrequency(Age);
        }

        private void UpdateHealthStatus()
        {
            if (RecommendedWeeklyCount <= 0)
            {
                RecommendedWeeklyCount = 3;
                return;
            }

            HealthStatus = _healthService.GetHealthStatus(WeekCount, RecommendedWeeklyCount);

            var progress = (double)WeekCount / RecommendedWeeklyCount;
            WeekProgress = Math.Min(progress, 1.0);
        }

        private void AddRecord()
        {
            try
            {
                var record = new Record();
                _recordService.AddRecord(record);
                // LoadData() will be called automatically via WeakReferenceMessenger
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"添加记录失败: {ex.Message}");
            }
        }

        private void DeleteRecord(Guid id)
        {
            try
            {
                _recordService.DeleteRecord(id);
                // LoadData() will be called automatically via WeakReferenceMessenger
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"删除记录失败: {ex.Message}");
            }
        }

        private void LoadData()
        {
            // 防止重复加载
            if (_isLoading) return;
            _isLoading = true;

            try
            {
                _lastRefreshTime = DateTime.Now;

                TotalCount = _recordService.GetTotalCount();
                TodayCount = _recordService.GetTodayCount();
                WeekCount = _recordService.GetWeekCount();
                MonthCount = _recordService.GetMonthCount();

                // 优化：只在数据真正变化时更新 ObservableCollection
                var allRecords = _recordService.GetAllRecords();

                // 批量更新以减少 UI 刷新次数
                UpdateRecordsCollection(Records, allRecords);
                UpdateRecordsCollection(RecentRecords, allRecords.Take(5).ToList());

                // 更新图表数据
                UpdateChartData();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"加载数据失败: {ex.Message}");
            }
            finally
            {
                _isLoading = false;
            }
        }

        private void UpdateRecordsCollection(ObservableCollection<Record> collection, IEnumerable<Record> newRecords)
        {
            var newList = newRecords.ToList();

            // 如果新旧数据都相同，不触发更新
            if (collection.Count == newList.Count &&
                collection.SequenceEqual(newList, new RecordComparer()))
            {
                return;
            }

            collection.Clear();
            foreach (var record in newList)
            {
                collection.Add(record);
            }
        }

        private void UpdateChartData()
        {
            UpdateWeeklyChart();
            UpdateMonthlyChart();
            UpdateHeatmap();
            UpdateLineChart();
        }

        private void UpdateWeeklyChart()
        {
            var weeklyStats = _recordService.GetLast7DaysStats();
            var maxCount = weeklyStats.Values.DefaultIfEmpty(0).Max();
            if (maxCount == 0) maxCount = 1;

            var dayNames = new[] { "日", "一", "二", "三", "四", "五", "六" };
            var today = DateTime.Today;

            WeeklyChartData.Clear();
            foreach (var kvp in weeklyStats)
            {
                var isToday = kvp.Key == today;
                WeeklyChartData.Add(new DailyStats
                {
                    Date = kvp.Key,
                    Count = kvp.Value,
                    DayLabel = dayNames[(int)kvp.Key.DayOfWeek],
                    BarHeight = Math.Max(4, (kvp.Value / (double)maxCount) * 100),
                    BarColor = isToday ? "#2196F3" : "#90CAF9",
                    IsToday = isToday
                });
            }
        }

        private void UpdateMonthlyChart()
        {
            var monthlyStats = _recordService.GetLast6MonthsStats();
            var maxCount = monthlyStats.Values.DefaultIfEmpty(0).Max();
            if (maxCount == 0) maxCount = 1;

            var thisMonth = (DateTime.Today.Year, DateTime.Today.Month);

            MonthlyChartData.Clear();
            foreach (var kvp in monthlyStats)
            {
                var isCurrentMonth = kvp.Key == thisMonth;
                MonthlyChartData.Add(new MonthlyStats
                {
                    Year = kvp.Key.Year,
                    Month = kvp.Key.Month,
                    Count = kvp.Value,
                    MonthLabel = $"{kvp.Key.Month}月",
                    BarWidth = Math.Max(4, (kvp.Value / (double)maxCount) * 100),
                    BarColor = isCurrentMonth ? "#4CAF50" : "#A5D6A7"
                });
            }
        }

        private void UpdateHeatmap()
        {
            var today = DateTime.Today;
            CurrentMonthLabel = $"{today.Year}年{today.Month}月";

            var monthlyStats = _recordService.GetCurrentMonthDailyStats();
            var maxCount = monthlyStats.Values.DefaultIfEmpty(0).Max();
            if (maxCount == 0) maxCount = 1;

            HeatmapData.Clear();

            var startOfMonth = new DateTime(today.Year, today.Month, 1);
            var daysInMonth = DateTime.DaysInMonth(today.Year, today.Month);

            // 添加每天的数据
            for (int day = 1; day <= daysInMonth; day++)
            {
                var date = new DateTime(today.Year, today.Month, day);
                var count = monthlyStats.TryGetValue(date, out var c) ? c : 0;

                // 根据数量设置颜色深度
                string cellColor;
                if (count == 0)
                {
                    cellColor = "#EEEEEE";
                }
                else if (count == 1)
                {
                    cellColor = "#C8E6C9";
                }
                else if (count == 2)
                {
                    cellColor = "#81C784";
                }
                else if (count == 3)
                {
                    cellColor = "#4CAF50";
                }
                else
                {
                    cellColor = "#2E7D32";
                }

                HeatmapData.Add(new HeatmapCell
                {
                    Date = date,
                    Count = count,
                    CellColor = cellColor,
                    DayOfMonth = day,
                    Tooltip = $"{date:MM/dd}: {count}次",
                    IsCurrentMonth = date <= today
                });
            }
        }

        private void UpdateLineChart()
        {
            var last30Days = _recordService.GetLast30DaysStats();
            var maxCount = last30Days.Values.DefaultIfEmpty(0).Max();
            if (maxCount == 0) maxCount = 1;

            const double chartWidth = 300;
            const double chartHeight = 100;
            var pointCount = last30Days.Count;
            var xStep = chartWidth / (pointCount - 1);

            LineChartData.Clear();
            int index = 0;
            foreach (var kvp in last30Days)
            {
                var x = index * xStep;
                var y = chartHeight - (kvp.Value / (double)maxCount) * chartHeight;

                LineChartData.Add(new LineChartPoint
                {
                    Date = kvp.Key,
                    Count = kvp.Value,
                    X = x,
                    Y = y,
                    DateLabel = kvp.Key.ToString("MM/dd")
                });
                index++;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // 记录比较器
        private class RecordComparer : IEqualityComparer<Record>
        {
            public bool Equals(Record? x, Record? y)
            {
                if (x == null || y == null) return false;
                return x.Id == y.Id;
            }

            public int GetHashCode(Record obj)
            {
                return obj.Id.GetHashCode();
            }
        }
    }
}
