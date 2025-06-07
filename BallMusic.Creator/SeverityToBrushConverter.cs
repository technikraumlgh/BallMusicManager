using BallMusic.Tips;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace BallMusic.Creator;

internal sealed class SeverityToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => value switch
    {
        Severity.Tip => Brushes.CornflowerBlue,
        Severity.Warning => Brushes.Yellow,
        Severity.Error => Brushes.Red,
        _ => Brushes.White,
    };

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException("Cannot convert from Brush to Severity");
    }
}
