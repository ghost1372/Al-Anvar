using AlAnvar.Models;
using Microsoft.UI.Xaml.Media;

namespace AlAnvar.Common;

public static partial class FontHelper
{
    public static List<double> FontSize { get; } = Enumerable.Range(6, 48).Select(x => (double)x).ToList();

    public static List<FontOption> UIFonts { get; } = new()
    {
        new FontOption("IRANSansXFont", "IRANSans"),
        new FontOption("IRANYekanFont", "IRANYekan"),
        new FontOption("VazirmatnFont", "Vazirmatn"),
    };
    public static List<FontOption> TranslationFonts { get; } = new()
    {
        new FontOption("IRANSansXFont", "IRANSans"),
        new FontOption("IRANYekanFont", "IRANYekan"),
        new FontOption("VazirmatnFont", "Vazirmatn"),
    };
    public static List<FontOption> QuranFonts { get; } = new()
    {
        new FontOption("IRANSansXFont", "IRANSans"),
        new FontOption("IRANYekanFont", "IRANYekan"),
        new FontOption("VazirmatnFont", "Vazirmatn"),
        new FontOption("AlkalamiFont", "Alkalami"),
        new FontOption("HarmattanFont", "Harmattan"),
        new FontOption("NabiFont", "Nabi"),
        new FontOption("NeiriziFont", "Neirizi"),
        new FontOption("QuranTahaFont", "QuranTaha"),
        new FontOption("RuwuduFont", "Ruwudu"),
        new FontOption("KufiFont", "Kufi"),
        new FontOption("UthmanicHafsFont", "UthmanicHafs"),
        new FontOption("UthmanTNFont", "UthmanTN"),
    };

    public static void SetUIFontFamily(FontOption font)
    {
        var uiFont = Application.Current.Resources[font.FontKey] as FontFamily;
        Application.Current.Resources["AlAnvarUIFont"] = new FontFamily(uiFont.Source);
        Application.Current.Resources["XamlAutoFontFamily"] = new FontFamily(uiFont.Source);
        Application.Current.Resources["ContentControlThemeFontFamily"] = new FontFamily(uiFont.Source);
    }
    public static void SetQuranFontFamily(FontOption font)
    {
        var quranFont = Application.Current.Resources[font.FontKey] as FontFamily;
        ProxyService.Instance.QuranFontFamily = quranFont;
    }
    public static void SetTranslationFontFamily(FontOption font)
    {
        var translationFont = Application.Current.Resources[font.FontKey] as FontFamily;
        ProxyService.Instance.TranslationFontFamily = translationFont;
    }
}
