using System.Globalization;

namespace zuoleme.Converters
{
    /// <summary>
    /// 将 Y 坐标转换为从底部算起的高度
    /// 用于折线图显示
    /// </summary>
    public class InvertedHeightConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is double y)
            {
                // 图表高度100，Y是从顶部算起的距离
                // 我们需要返回从底部算起的高度
                return Math.Max(0, 100 - y);
            }
            return 0;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
