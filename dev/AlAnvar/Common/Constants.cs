using AlAnvar.Models;
using Microsoft.UI.Xaml.Media;

namespace AlAnvar.Common;

public static partial class Constants
{
    public static readonly string RootDirectoryPath = Path.Combine(PathHelper.GetAppDataFolderPath(), ProcessInfoHelper.ProductName);
    public static readonly string LogDirectoryPath = Path.Combine(RootDirectoryPath, "Log");
    public static readonly string LogFilePath = Path.Combine(LogDirectoryPath, "Log.txt");
    public static readonly string AppConfigPath = Path.Combine(RootDirectoryPath, "AppConfig.json");

    public static readonly string AudiosPath = Path.Combine(RootDirectoryPath, "Audio");
    public static readonly string TranslationsPath = Path.Combine(RootDirectoryPath, "Translations");
    public static readonly string DatabaseFilePath = Path.Combine(AppContext.BaseDirectory, "Assets", "DataBase", "Al-Anvar.db");

    public static readonly string RepoName = "Al-Anvar";
    public static readonly string Username = "Ghost1372";
    public static readonly string RepoUrl = "https://github.com/Ghost1372/Al-Anvar";
    public static readonly string RepoReleaseUrl = "https://github.com/Ghost1372/Al-Anvar/releases";

    public static readonly string DefaultLanguage = "fa-IR";

    public static readonly double DefaultQuranFontSize = 14.0;
    public static readonly double DefaultTranslationFontSize = 14.0;
    public static readonly FontOption DefaultUIFont = FontHelper.UIFonts.LastOrDefault();
    public static readonly FontOption DefaultQuranFont = FontHelper.UIFonts.LastOrDefault();
    public static readonly FontOption DefaultTranslationFont = FontHelper.TranslationFonts.LastOrDefault();
    public static SolidColorBrush DefaultTextBrush => Application.Current.Resources["TextFillColorPrimaryBrush"] as SolidColorBrush;
}
