using System.Collections.ObjectModel;

namespace AlAnvar.ViewModels;

public partial class TranslationSettingViewModel : ObservableRecipient
{
    private readonly DispatcherQueue dispatcherQueue = DispatcherQueue.GetForCurrentThread();

    [ObservableProperty]
    public ObservableCollection<QuranTranslation> translationsCollection = new();

    [ObservableProperty]
    public QuranTranslation currentTranslation = Settings.QuranTranslation;

    [ObservableProperty]
    public int translationIndex = -1;

    public TranslationSettingViewModel()
    {
        LoadTranslationsAsync();
    }

    private async void LoadTranslationsAsync()
    {
        IsActive = true;
        await Task.Run(async () =>
        {
            using var db = new AlAnvarDBContext();
            var data = await db.Translations.Where(x=>x.IsActive).ToListAsync();
            dispatcherQueue.TryEnqueue(() =>
            {
                TranslationsCollection = new(data);
                if (Settings.QuranTranslation != null)
                {
                    CurrentTranslation = TranslationsCollection.FirstOrDefault(x => x.Id == Settings.QuranTranslation.Id);
                    TranslationIndex = TranslationsCollection.IndexOf(CurrentTranslation);
                }
            });
        });
        IsActive = false;
    }

    [RelayCommand]
    private void OnTranslationItemChanged(object sender)
    {
        var cmbTranslator = sender as ComboBox;
        if (cmbTranslator.SelectedItem != null)
        {
            Settings.QuranTranslation = cmbTranslator.SelectedItem as QuranTranslation;
        }
    }
}
