using System.Globalization;

namespace ECHA.Mobile.Converters;

public class BoolToFavoritoConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return (bool?)value == true ? "Remover dos Favoritos" : "Adicionar aos Favoritos";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
