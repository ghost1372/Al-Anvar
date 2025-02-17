using DevWinUI;

namespace AlAnvar.Common;
public static class Constants
{
    public const string ALAnvar_Repo = "https://github.com/ghost1372/Al-Anvar/releases";

    public static readonly string AppName = ProcessInfoHelper.ProductNameAndVersion;
    public static readonly string RootDirectoryPath = Path.Combine(PathHelper.GetAppDataFolderPath(), ProcessInfoHelper.ProductNameAndVersion);
    public static readonly string AppConfigPath = Path.Combine(RootDirectoryPath, "AppConfig.json");

    public static readonly string LogDirectoryPath = Path.Combine(RootDirectoryPath, "Log");
    public static readonly string LogFilePath = Path.Combine(LogDirectoryPath, "Log.txt");

    public static readonly string AudiosFolderName = "Audio";
    public static readonly string AudiosPath = Path.Combine(RootDirectoryPath, AudiosFolderName);
    public static readonly string AppGithubPage = "https://github.com/ghost1372/Al-Anvar";
    public static readonly string TafsirTabViewItemHeader = "تفسیر قرآن";

    public const double AYAT_DEFAULT_FONT_SIZE = 34;
    public const double AYAT_NUMBER_DEFAULT_FONT_SIZE = 26;
    public const double TRANSLATION_DEFAULT_FONT_SIZE = 20;
    public const string AYAT_DEFAULT_FONT_NAME = "ms-appx:///Assets/Fonts/UthmanTN_v2-0.ttf#KFGQPC Uthman Taha Naskh";
    public const string AYAT_NUMBER_DEFAULT_FONT_NAME = "ms-appx:///Assets/Fonts/Nabi.ttf#Nabi";
    public const string TRANSLATION_DEFAULT_FONT_NAME = "ms-appx:///Assets/Fonts/IRANYekanRegular.ttf#IRANYekan";
}
