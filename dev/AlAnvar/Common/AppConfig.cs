using AlAnvar.Models;
using Nucs.JsonSettings.Examples;
using Nucs.JsonSettings.Modulation;

namespace AlAnvar.Common;

[GenerateAutoSaveOnChange]
public partial class AppConfig : NotifiyingJsonSettings, IVersionable
{
    [EnforcedVersion("3.0.0.2")]
    public Version Version { get; set; } = new Version(3, 0, 0, 2);

    private string fileName { get; set; } = Constants.AppConfigPath;
    private string dBPath { get; set; } = Constants.DatabaseFilePath;
    private string lastUpdateCheck { get; set; }
    private string audiosPath { get; set; } = Constants.AudiosPath;
    private string translationsPath { get; set; } = Constants.TranslationsPath;

    private bool useDeveloperMode { get; set; } = true;
    private bool isAutoDownloadAudio { get; set; } = true;
    private bool isFirstRun { get; set; } = true;

    private int audioIndex { get; set; }
    private int translationIndex { get; set; }

    private TranslationItem translation { get; set; }
    private AudioItem audio { get; set; }
    private FontOption uIFont { get; set; }
    private FontOption quranFont { get; set; }
    private FontOption translationFont { get; set; }
    private TabViewWidthMode tabWidthMode { get; set; } = TabViewWidthMode.SizeToContent;
}
