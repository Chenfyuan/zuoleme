using System.Globalization;

namespace zuoleme.Converters
{
    /// <summary>
    /// 判断字符串是否非空，用于控制备注文本的可见性
    /// </summary>
    public class StringNotEmptyConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return !string.IsNullOrWhiteSpace(value as string);
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
