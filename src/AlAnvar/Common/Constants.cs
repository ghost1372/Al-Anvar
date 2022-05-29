namespace AlAnvar.Common;

public class Constants
{
    public static readonly string AppName = "AlAnvar1.0";
    public static readonly string RootPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), AppName);
    public static readonly string TranslationsFolderName = "Translations";
    public static readonly string AudiosFolderName = "Audio";
    public static readonly string AppConfigPath = Path.Combine(RootPath, "AppConfig.json");
    public static readonly string AppGithubPage = "https://github.com/ghost1372/Al-Anvar";
    public static readonly string TafsirTabViewItemHeader = "تفسیر قرآن";

    public const double AYAT_DEFAULT_FONT_SIZE = 24;
    public const double AYAT_NUMBER_DEFAULT_FONT_SIZE = 24;
    public const double TRANSLATION_DEFAULT_FONT_SIZE = 24;
    public const string AYAT_DEFAULT_FONT_NAME = "Assets/Fonts/Vazirmatn-Regular.ttf#Vazirmatn";
    public const string AYAT_NUMBER_DEFAULT_FONT_NAME = "Assets/Fonts/Vazirmatn-Regular.ttf#Vazirmatn";
    public const string TRANSLATION_DEFAULT_FONT_NAME = "Assets/Fonts/Vazirmatn-Regular.ttf#Vazirmatn";

    public const string FONT_VAZIRMATN_REGULAR = "Assets/Fonts/Vazirmatn-Regular.ttf#Vazirmatn";
    public const string FONT_VAZIRMATN_MEDIUM = "Assets/Fonts/Vazirmatn-Medium.ttf#Vazirmatn Medium";
    public const string FONT_VAZIRMATN_BOLD = "Assets/Fonts/Vazirmatn-Bold.ttf#Vazirmatn";
}
