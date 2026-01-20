using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using zuoleme.Models;
using zuoleme.Services;

namespace zuoleme.ViewModels
{
    public class CalendarViewModel : INotifyPropertyChanged
    {
        private readonly RecordService _recordService;
        
        private DateTime _currentMonth;
        private int _selectedMonthRecordCount;
        
        public DateTime CurrentMonth
        {
            get => _currentMonth;
            set
            {
                _currentMonth = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(MonthYearDisplay));
                LoadCalendarData();
            }
        }

        public string MonthYearDisplay => CurrentMonth.ToString("yyyy年 MM月");

        public int SelectedMonthRecordCount
        {
            get => _selectedMonthRecordCount;
            set
            {
                _selectedMonthRecordCount = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<CalendarDay> CalendarDays { get; set; }

        public ICommand PreviousMonthCommand { get; }
        public ICommand NextMonthCommand { get; }
        public ICommand TodayCommand { get; }

        public CalendarViewModel(RecordService recordService)
        {
            _recordService = recordService;
            CalendarDays = new ObservableCollection<CalendarDay>();
            
            CurrentMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            
            PreviousMonthCommand = new Command(PreviousMonth);
            NextMonthCommand = new Command(NextMonth);
            TodayCommand = new Command(GoToToday);

            LoadCalendarData();
        }

        private void PreviousMonth()
        {
            CurrentMonth = CurrentMonth.AddMonths(-1);
        }

        private void NextMonth()
        {
            CurrentMonth = CurrentMonth.AddMonths(1);
        }

        private void GoToToday()
        {
            CurrentMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
        }

        private void LoadCalendarData()
        {
            CalendarDays.Clear();

            var firstDayOfMonth = new DateTime(CurrentMonth.Year, CurrentMonth.Month, 1);
            var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);
            
            // 获取该月份的所有记录
            var allRecords = _recordService.GetAllRecords();
            var monthRecords = allRecords.Where(r => 
                r.Timestamp.Year == CurrentMonth.Year && 
                r.Timestamp.Month == CurrentMonth.Month).ToList();
            
            SelectedMonthRecordCount = monthRecords.Count;

            // 计算第一天是星期几（0=Sunday, 1=Monday, etc.）
            int firstDayOfWeek = (int)firstDayOfMonth.DayOfWeek;
            
            // 添加上个月的空白天数
            var previousMonth = firstDayOfMonth.AddMonths(-1);
            var daysInPreviousMonth = DateTime.DaysInMonth(previousMonth.Year, previousMonth.Month);
            for (int i = firstDayOfWeek - 1; i >= 0; i--)
            {
                var day = new CalendarDay
                {
                    Date = new DateTime(previousMonth.Year, previousMonth.Month, daysInPreviousMonth - i),
                    IsCurrentMonth = false,
                    RecordCount = 0
                };
                CalendarDays.Add(day);
            }

            // 添加当月的所有天数
            for (int day = 1; day <= lastDayOfMonth.Day; day++)
            {
                var date = new DateTime(CurrentMonth.Year, CurrentMonth.Month, day);
                var dayRecords = monthRecords.Where(r => r.Timestamp.Date == date.Date).ToList();
                
                var calendarDay = new CalendarDay
                {
                    Date = date,
                    IsCurrentMonth = true,
                    IsToday = date.Date == DateTime.Today,
                    RecordCount = dayRecords.Count,
                    Records = new ObservableCollection<Record>(dayRecords)
                };
                
                CalendarDays.Add(calendarDay);
            }

            // 添加下个月的空白天数（填充到满足6周）
            int remainingDays = 42 - CalendarDays.Count; // 6周 * 7天 = 42天
            var nextMonth = firstDayOfMonth.AddMonths(1);
            for (int day = 1; day <= remainingDays; day++)
            {
                var calendarDay = new CalendarDay
                {
                    Date = new DateTime(nextMonth.Year, nextMonth.Month, day),
                    IsCurrentMonth = false,
                    RecordCount = 0
                };
                CalendarDays.Add(calendarDay);
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class CalendarDay : INotifyPropertyChanged
    {
        private DateTime _date;
        private bool _isCurrentMonth;
        private bool _isToday;
        private int _recordCount;

        public DateTime Date
        {
            get => _date;
            set
            {
                _date = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(DayNumber));
            }
        }

        public string DayNumber => Date.Day.ToString();

        public bool IsCurrentMonth
        {
            get => _isCurrentMonth;
            set
            {
                _isCurrentMonth = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TextOpacity));
            }
        }

        public bool IsToday
        {
            get => _isToday;
            set
            {
                _isToday = value;
                OnPropertyChanged();
            }
        }

        public int RecordCount
        {
            get => _recordCount;
            set
            {
                _recordCount = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasRecords));
                OnPropertyChanged(nameof(RecordCountDisplay));
                OnPropertyChanged(nameof(IntensityLevel));
            }
        }

        public bool HasRecords => RecordCount > 0;
        public string RecordCountDisplay => RecordCount > 0 ? RecordCount.ToString() : "";
        public double TextOpacity => IsCurrentMonth ? 1.0 : 0.3;
        
        // 根据记录数量返回强度等级（用于颜色深浅）
        public int IntensityLevel
        {
            get
            {
                if (RecordCount == 0) return 0;
                if (RecordCount == 1) return 1;
                if (RecordCount == 2) return 2;
                if (RecordCount >= 3) return 3;
                return 0;
            }
        }

        public ObservableCollection<Record> Records { get; set; } = new ObservableCollection<Record>();

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
