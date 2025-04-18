﻿using BallMusic.Tips;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace BallMusicManager.Creator;

internal sealed class SeverityToColorConverter : IValueConverter
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
        throw new NotImplementedException();
    }
}
