using Microsoft.UI.Xaml.Data;

namespace AlAnvar.Tools.Converters;
public class ThumbToolTipValueConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return QuranPage.Instance.GetPositionTimeFormat();
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
