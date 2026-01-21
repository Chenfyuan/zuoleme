namespace zuoleme.Models
{
    public class DailyStats
    {
        public DateTime Date { get; set; }
        public int Count { get; set; }
        public string DayLabel { get; set; } = "";
        public double BarHeight { get; set; }
        public string BarColor { get; set; } = "#2196F3";
        public bool IsToday { get; set; }
    }

    public class MonthlyStats
    {
        public int Month { get; set; }
        public int Year { get; set; }
        public int Count { get; set; }
        public string MonthLabel { get; set; } = "";
        public double BarWidth { get; set; }
        public string BarColor { get; set; } = "#4CAF50";
    }

    /// <summary>
    /// 热力图单元格数据
    /// </summary>
    public class HeatmapCell
    {
        public DateTime Date { get; set; }
        public int Count { get; set; }
        public string CellColor { get; set; } = "#EEEEEE";
        public double Opacity { get; set; } = 1.0;
        public string Tooltip { get; set; } = "";
        public int DayOfMonth { get; set; }
        public bool IsCurrentMonth { get; set; } = true;
    }

    /// <summary>
    /// 热力图一周行数据
    /// </summary>
    public class HeatmapWeek
    {
        public List<HeatmapCell> Days { get; set; } = new();
    }

    /// <summary>
    /// 折线图数据点
    /// </summary>
    public class LineChartPoint
    {
        public DateTime Date { get; set; }
        public int Count { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public string DateLabel { get; set; } = "";
    }
}
