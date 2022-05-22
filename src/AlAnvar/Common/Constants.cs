namespace AlAnvar.Common;

public class Constants
{
    public static readonly string AppName = "AlAnvar1.0";
    public static readonly string RootPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), AppName);
    public static readonly string TranslationsPath = Path.Combine(RootPath, "Translations");
    public static readonly string AppConfigPath = Path.Combine(RootPath, "AppConfig.json");
}