using DevWinUI;
using Nucs.JsonSettings.Examples;
using Nucs.JsonSettings.Modulation;

namespace AlAnvar.Common;

[GenerateAutoSaveOnChange]
public partial class AlAnvarSettings : NotifiyingJsonSettings, IVersionable
{
    [EnforcedVersion("2.4.0.0")]
    public virtual Version Version { get; set; } = new Version(2, 4, 0, 0);
    private string fileName { get; set; } = AppConfigPath;

    private bool useDeveloperMode { get; set; }
    private bool isUsingSystemFonts { get; set; } = false;
    private string ayatForeground { get; set; }
    private string translationForeground { get; set; }
    private string ayatNumberForeground { get; set; }
    public virtual string ayatFontFamilyName { get; set; } = AYAT_DEFAULT_FONT_NAME;
    private string translationFontFamilyName { get; set; } = TRANSLATION_DEFAULT_FONT_NAME;
    private string ayatNumberFontFamilyName { get; set; } = AYAT_NUMBER_DEFAULT_FONT_NAME;
    private double ayatFontSize { get; set; } = AYAT_DEFAULT_FONT_SIZE;
    private double translationFontSize { get; set; } = TRANSLATION_DEFAULT_FONT_SIZE;
    private double ayatNumberFontSize { get; set; } = AYAT_NUMBER_DEFAULT_FONT_SIZE;
    private QuranTranslation quranTranslation { get; set; }
    private QuranAudio quranAudio { get; set; }
    private string lastUpdateCheck { get; set; } = "هرگز";
    private bool isAutoDownloadSound { get; set; } = true;
    private string audiosPath { get; set; } = Constants.AudiosPath;
    private TextAlignment textAlignment { get; set; } = TextAlignment.Center;
    private FocusState focusState { get; set; } = FocusState.Keyboard;
}
