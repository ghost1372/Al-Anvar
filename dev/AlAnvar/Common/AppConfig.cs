using AlAnvar.Models;
using Nucs.JsonSettings;
using Nucs.JsonSettings.Autosave;
using Nucs.JsonSettings.Modulation;

namespace AlAnvar.Common;

[Autosave]
public partial class AppConfig : NotifiyingJsonSettings, IVersionable
{
    [EnforcedVersion("3.0.0.2")]
    public Version Version { get; set; } = new Version(3, 0, 0, 2);

    public override string FileName { get; set; } = Constants.AppConfigPath;
    public string DBPath { get; set; } = Constants.DatabaseFilePath;
    public string LastUpdateCheck { get; set; }
    public string AudiosPath { get; set; } = Constants.AudiosPath;
    public string TranslationsPath { get; set; } = Constants.TranslationsPath;

    public bool UseDeveloperMode { get; set; } = true;
    public bool IsAutoDownloadAudio { get; set; } = true;
    public bool IsFirstRun { get; set; } = true;

    public int AudioIndex { get; set; }
    public int TranslationIndex { get; set; }

    public TranslationItem Translation { get; set; }
    public AudioItem Audio { get; set; }
    public FontOption UIFont { get; set; }
    public FontOption QuranFont { get; set; }
    public FontOption TranslationFont { get; set; }
    public TabViewWidthMode TabWidthMode { get; set; } = TabViewWidthMode.SizeToContent;
}
