using Nucs.JsonSettings;
using Nucs.JsonSettings.Modulation;

namespace AlAnvar.Helpers;
public class AlAnvarSettings : JsonSettings, IVersionable
{
    [EnforcedVersion("2.2.0.0")]
    public virtual Version Version { get; set; } = new Version(2, 2, 0, 0);
    public override string FileName { get; set; } = AppConfigPath;

    public virtual bool UseDeveloperMode { get; set; }
    public virtual bool IsUsingSystemFonts { get; set; } = false;
    public virtual string AyatForeground { get; set; }
    public virtual string TranslationForeground { get; set; }
    public virtual string AyatNumberForeground { get; set; }
    public virtual string AyatFontFamilyName { get; set; } = AYAT_DEFAULT_FONT_NAME;
    public virtual string TranslationFontFamilyName { get; set; } = TRANSLATION_DEFAULT_FONT_NAME;
    public virtual string AyatNumberFontFamilyName { get; set; } = AYAT_NUMBER_DEFAULT_FONT_NAME;
    public virtual double AyatFontSize { get; set; } = AYAT_DEFAULT_FONT_SIZE;
    public virtual double TranslationFontSize { get; set; } = TRANSLATION_DEFAULT_FONT_SIZE;
    public virtual double AyatNumberFontSize { get; set; } = AYAT_NUMBER_DEFAULT_FONT_SIZE;
    public virtual QuranTranslation QuranTranslation { get; set; }
    public virtual QuranAudio QuranAudio { get; set; }
    public virtual string LastUpdateCheck { get; set; } = "هرگز";
    public virtual bool IsAutoDownloadSound { get; set; } = true;
    public virtual string AudiosPath { get; set; } = Constants.AudiosPath;
    public virtual NavigationViewPaneDisplayMode PaneDisplayMode { get; set; } = NavigationViewPaneDisplayMode.Top;
    public virtual TextAlignment TextAlignment { get; set; } = TextAlignment.Center;
    public virtual FocusState FocusState { get; set; } = FocusState.Keyboard;
}
