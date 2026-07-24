using System.Globalization;
using Microsoft.Maui.Graphics;

namespace ECHA.Mobile.Converters;

public class TabColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var active = value?.ToString()?.ToLowerInvariant() ?? "";
        var tab = parameter?.ToString()?.ToLowerInvariant() ?? "";
        return active == tab
            ? Color.FromArgb("#570013")
            : Color.FromArgb("#919191");
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

public class TabHighlightConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var active = value?.ToString()?.ToLowerInvariant() ?? "";
        var tab = parameter?.ToString()?.ToLowerInvariant() ?? "";
        return active == tab
            ? Color.FromArgb("#FFDADA")
            : Colors.Transparent;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
