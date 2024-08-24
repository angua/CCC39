using System;
using System.Globalization;
using System.Windows.Data;

namespace CommonWPF;

internal class PositionConverter : IValueConverter
{
    public double Size { get; set; }
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return Size * (int)value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
