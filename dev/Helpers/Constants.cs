namespace AlAnvar.Helpers;
public static class Constants
{
    public const string ALAnvar_Repo = "https://github.com/ghost1372/Al-Anvar/releases";

    public static readonly string AppName = ApplicationHelper.GetAppNameAndVersion().NameAndVersion;
    public static readonly string RootDirectoryPath = Path.Combine(ApplicationHelper.GetLocalFolderPath(), AppName);
    public static readonly string LogDirectoryPath = Path.Combine(RootDirectoryPath, "Log");
    public static readonly string LogFilePath = Path.Combine(LogDirectoryPath, "Log.txt");
    public static readonly string AppConfigPath = Path.Combine(RootDirectoryPath, "AppConfig.json");

    public static readonly string AudiosFolderName = "Audio";
    public static readonly string AudiosPath = Path.Combine(RootDirectoryPath, AudiosFolderName);
    public static readonly string AppGithubPage = "https://github.com/ghost1372/Al-Anvar";
    public static readonly string TafsirTabViewItemHeader = "تفسیر قرآن";

    public const double AYAT_DEFAULT_FONT_SIZE = 22;
    public const double AYAT_NUMBER_DEFAULT_FONT_SIZE = 23;
    public const double TRANSLATION_DEFAULT_FONT_SIZE = 20;
    public const string AYAT_DEFAULT_FONT_NAME = IRANSANS_FONT_ASSET;
    public const string AYAT_NUMBER_DEFAULT_FONT_NAME = IRANSANS_FONT_ASSET;
    public const string TRANSLATION_DEFAULT_FONT_NAME = IRANSANS_FONT_ASSET;

    public const string IRANSANS_FONT_ASSET = "ms-appx:///Assets/Fonts/IRANSansX-Regular.ttf#IRANSansX";
    public const string VAZIRMATN_FONT_ASSET = "ms-appx:///Assets/Fonts/Vazirmatn-Regular.ttf#Vazirmatn";
    public const string IRANYEKAN_FONT_ASSET = "ms-appx:///Assets/Fonts/IRANYekanRegular.ttf#IRANYekan";
    public const string NABI_FONT_ASSET = "ms-appx:///Assets/Fonts/Nabi.ttf#Nabi";
    public const string NEIRIZI_FONT_ASSET = "ms-appx:///Assets/Fonts/Neirizi.ttf#Neirizi";
    public const string QURANTAHA_FONT_ASSET = "ms-appx:///Assets/Fonts/QuranTaha.ttf#QuranTaha";

    public const string IRANSANS_FONT_PERSIAN = "ایران سنس";
    public const string VAZIRMATN_FONT_PERSIAN = "وزیر متن";
    public const string IRANYEKAN_FONT_PERSIAN = "ایران یکان";
    public const string NABI_FONT_PERSIAN = "نبی";
    public const string NEIRIZI_FONT_PERSIAN = "نیریزی";
    public const string QURANTAHA_FONT_PERSIAN = "قرآن طه";
}
