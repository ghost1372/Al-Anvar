using Microsoft.UI.Xaml.Data;

namespace AlAnvar.Tools.Converters;
public class ThumbToolTipValueConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        var instance = QuranPage.Instance;
        if (instance is not null && instance.GetTabView().SelectedItem is not null)
        {
            var tabItem = instance.GetTabView().SelectedItem as QuranTabViewItem;
            return tabItem.GetPositionTimeFormat();
        }
        return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
