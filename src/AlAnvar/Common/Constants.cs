namespace AlAnvar.Common;

public class Constants
{
    public static readonly string AppName = "AlAnvar1.0";
    public static readonly string RootPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), AppName);
    public static readonly string TranslationsPath = Path.Combine(RootPath, "Translations");
    public static readonly string AppConfigPath = Path.Combine(RootPath, "AppConfig.json");

    public const double AYAT_DEFAULT_FONT_SIZE = 24;
    public const double AYAT_NUMBER_DEFAULT_FONT_SIZE = 24;
    public const double TRANSLATION_DEFAULT_FONT_SIZE = 24;
}
