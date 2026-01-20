using System.Globalization;

namespace zuoleme.Converters
{
    public class ProgressToWidthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double progress)
            {
                // 计算进度条宽度为屏幕宽度减去 padding (48px)
                var screenWidth = DeviceDisplay.MainDisplayInfo.Width / DeviceDisplay.MainDisplayInfo.Density;
                var containerWidth = screenWidth - 88; // 减去两侧 padding 和边距
                
                // 计算进度宽度，最大为容器宽度
                return Math.Min(progress * containerWidth / 2, containerWidth);
            }
            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
