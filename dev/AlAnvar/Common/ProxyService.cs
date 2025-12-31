using Microsoft.UI.Xaml.Media;

namespace AlAnvar.Common;

public partial class ProxyService : ObservableObject
{
    private static readonly Lazy<ProxyService> _instance = new(() => new ProxyService());
    public static ProxyService Instance => _instance.Value;
    private ProxyService() { }

    [ObservableProperty]
    public partial bool IsAyaVisible { get; set; } = true;

    [ObservableProperty]
    public partial bool IsTranslationVisible { get; set; } = true;

    [ObservableProperty]
    public partial bool IsDiacriticsVisible { get; set; } = true;

    [ObservableProperty]
    public partial bool IsOperationButtonVisible { get; set; } = true;

    [ObservableProperty]
    public partial bool IsUserDefinedQuranColor { get; set; } = false;
    [ObservableProperty]
    public partial bool IsUserDefinedTranslationColor { get; set; } = false;
    [ObservableProperty]
    public partial bool IsUserDefinedQuranNumberColor { get; set; } = false;

    [ObservableProperty]
    public partial TextAlignment VerseTextAlignment { get; set; } = TextAlignment.Right;

    [ObservableProperty]
    public partial TextAlignment TranslationTextAlignment { get; set; } = TextAlignment.Right;

    [ObservableProperty]
    public partial double QuranFontSize { get; set; } = Constants.DefaultQuranFontSize;

    [ObservableProperty]
    public partial FontFamily QuranFontFamily { get; set; }

    [ObservableProperty]
    public partial SolidColorBrush? QuranColor { get; set; } = Constants.DefaultTextBrush;
    partial void OnQuranColorChanged(SolidColorBrush? value)
    {
        if (value == null || value.Color == Colors.Transparent)
        {
            IsUserDefinedQuranColor = false;
            QuranColor = Constants.DefaultTextBrush;
        }
        else
        {
            IsUserDefinedQuranColor = true;
        }
    }

    [ObservableProperty]
    public partial double TranslationFontSize { get; set; } = Constants.DefaultTranslationFontSize;

    [ObservableProperty]
    public partial FontFamily TranslationFontFamily { get; set; }

    [ObservableProperty]
    public partial SolidColorBrush? TranslationColor { get; set; } = Constants.DefaultTextBrush;
    partial void OnTranslationColorChanged(SolidColorBrush? value)
    {
        if (value == null || value.Color == Colors.Transparent)
        {
            IsUserDefinedTranslationColor = false;
            TranslationColor = Constants.DefaultTextBrush;
        }
        else
        {
            IsUserDefinedTranslationColor = true;
        }
    }

    [ObservableProperty]
    public partial SolidColorBrush? QuranNumberColor { get; set; } = Constants.DefaultTextBrush;
    partial void OnQuranNumberColorChanged(SolidColorBrush? value)
    {
        if (value == null || value.Color == Colors.Transparent)
        {
            IsUserDefinedQuranNumberColor = false;
            QuranNumberColor = Constants.DefaultTextBrush;
        }
        else
        {
            IsUserDefinedQuranNumberColor = true;
        }
    }
}
