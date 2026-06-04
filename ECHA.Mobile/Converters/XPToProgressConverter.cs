using System.Globalization;
using ECHA.Mobile.Models;

namespace ECHA.Mobile.Converters;

public class XPToProgressConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        // This converter needs access to UserStatsDto to calculate ratio
        // For simplicity, assuming caller passes the stats object or handles ratio in VM
        return 0.5; // Dummy for now
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotImplementedException();
}
