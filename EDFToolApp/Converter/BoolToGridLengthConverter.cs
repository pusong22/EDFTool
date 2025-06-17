using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace EDFToolApp.Converter;

public class BoolToGridLengthConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return (bool)value ? new GridLength(200) : new GridLength(40);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var gl = (GridLength)value;
        return gl.Value > 40;
    }
}
