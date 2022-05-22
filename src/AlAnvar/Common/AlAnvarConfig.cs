using Nucs.JsonSettings;
using Nucs.JsonSettings.Modulation;

namespace AlAnvar.Common;

public class AlAnvarConfig : JsonSettings, IVersionable
{
    [EnforcedVersion("1.1.0.0")]
    public virtual Version Version { get; set; } = new Version(1, 1, 0, 0);
    public override string FileName { get; set; } = Constants.AppConfigPath;

    public virtual bool IsUsingSystemFonts { get; set; } = false;
    public virtual string AyatForeground { get; set; }
    public virtual string TranslationForeground { get; set; }
    public virtual string AyatNumberForeground { get; set; }
    public virtual string AyatFontFamilyName { get; set; }
    public virtual string TranslationFontFamilyName { get; set; }
    public virtual string AyatNumberFontFamilyName { get; set; }
    public virtual double AyatFontSize { get; set; } = Constants.AYAT_DEFAULT_FONT_SIZE;
    public virtual double TranslationFontSize { get; set; } = Constants.TRANSLATION_DEFAULT_FONT_SIZE;
    public virtual double AyatNumberFontSize { get; set; } = Constants.AYAT_NUMBER_DEFAULT_FONT_SIZE;
    public virtual QuranTranslation QuranTranslation { get; set; }
    public virtual string LastUpdateCheck { get; set; } = "هرگز";

}
