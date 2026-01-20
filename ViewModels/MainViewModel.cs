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
                OnPropertyChanged(nameof(HealthProgressColor));
                OnPropertyChanged(nameof(HealthBackgroundStartColor));
                OnPropertyChanged(nameof(HealthBackgroundEndColor));
                OnPropertyChanged(nameof(ButtonBackgroundColor));
            }
        }

        public string HealthStatusTitle => HealthStatus?.Title ?? "健康状态";
        public string HealthStatusMessage => HealthStatus?.Message ?? "状态正常";
        public string HealthStatusColor => HealthStatus?.Color ?? "#2196F3";
        public string HealthStatusIcon => HealthStatus?.Icon ?? "\ue88e";
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

        public ObservableCollection<Record> Records { get; set; }
        public ObservableCollection<Record> RecentRecords { get; set; }

        public ICommand AddRecordCommand { get; }
        public ICommand DeleteRecordCommand { get; }
        public ICommand RefreshCommand { get; }

        public MainViewModel(RecordService recordService, HealthService healthService)
        {
            _recordService = recordService;
            _healthService = healthService;
            Records = new ObservableCollection<Record>();
            RecentRecords = new ObservableCollection<Record>();
            
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
