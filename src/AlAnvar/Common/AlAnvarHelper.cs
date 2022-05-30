using Nucs.JsonSettings;
using Nucs.JsonSettings.Autosave;
using Nucs.JsonSettings.Fluent;
using Nucs.JsonSettings.Modulation;
using Nucs.JsonSettings.Modulation.Recovery;

namespace AlAnvar.Common;

public static class AlAnvarHelper
{
    public static AlAnvarConfig Settings = JsonSettings.Configure<AlAnvarConfig>()
                               .WithRecovery(RecoveryAction.RenameAndLoadDefault)
                               .WithVersioning(VersioningResultAction.RenameAndLoadDefault)
                               .LoadNow()
                               .EnableAutosave();

    public static T GetComboboxFirstElement<T>(ComboBox comboBox)
    {
        comboBox.SelectedIndex = 0;
        return (T) Convert.ChangeType(comboBox.SelectedItem, typeof(T));
    }
    public static List<TranslationItem> GetQuranTranslation(ComboBox comboBox)
    {
        List<TranslationItem> TranslationCollection = new List<TranslationItem>();

        var selectedTranslation = Settings.QuranTranslation ?? GetComboboxFirstElement<QuranTranslation>(comboBox);
        if (Directory.Exists(Settings.TranslationsPath) && selectedTranslation is not null)
        {
            var files = Directory.GetFiles(Settings.TranslationsPath, "*.txt", SearchOption.AllDirectories);
            if (files.Count() > 0)
            {
                foreach (var item in files)
                {
                    if (Path.GetFileNameWithoutExtension(item).Equals(selectedTranslation.TranslationId))
                    {
                        using (var streamReader = File.OpenText(item))
                        {
                            string line = String.Empty;
                            while ((line = streamReader.ReadLine()) != null)
                            {
                                var trans = line.Split("|");
                                if (trans.Length > 1)
                                {
                                    TranslationCollection.Add(new TranslationItem
                                    {
                                        SurahId = Convert.ToInt32(trans[0]),
                                        Aya = Convert.ToInt32(trans[1]),
                                        Translation = trans[2]
                                    });
                                }
                            }
                        }
                    }
                }
            }
        }
        return TranslationCollection;
    }
}
