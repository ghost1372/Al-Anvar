using System.Collections.ObjectModel;

namespace AlAnvar.ViewModels;
public partial class OfflineTranslationViewModel : ObservableRecipient
{
    [ObservableProperty]
    private ObservableCollection<QuranTranslation> quranTranslations;

    [ObservableProperty]
    public AdvancedCollectionView quranTranslationsACV;

    [ObservableProperty]
    public object listViewSelectedItem;

    public IList<object> listViewSelectedItems;

    public OfflineTranslationViewModel()
    {
        GetLocalTranslations();
    }

    [RelayCommand]
    private void OnListViewItemChanged(object sender)
    {
        var listview = sender as ListView;
        listViewSelectedItems = listview?.SelectedItems;
        IsActive = listViewSelectedItems != null && listViewSelectedItems.Count > 0;
    }

    [RelayCommand]
    private void OnRemoveTranslation()
    {
        IsActive = false;
        DeleteTranslation();
    }

    [RelayCommand]
    private void OnRefresh()
    {
        GetLocalTranslations();
    }

    [RelayCommand]
    private async void DeleteTranslation()
    {
        if (listViewSelectedItems != null && listViewSelectedItems.Count > 0)
        {
            using var db = new AlAnvarDBContext();
            foreach (var item in listViewSelectedItems)
            {
                var translationId = (item as QuranTranslation).TranslationId;
                var updateTranslation = await db.Translations.Where(x => x.TranslationId.Equals(translationId)).FirstOrDefaultAsync();
                updateTranslation.IsActive = false;
                var removeTranslation = await db.TranslationsText.Where(x => x.TranslationId.Equals(translationId)).FirstOrDefaultAsync();
                db.TranslationsText.Remove(removeTranslation);
            }
            await db.SaveChangesAsync();
            GetLocalTranslations();
        }
    }

    private async void GetLocalTranslations()
    {
        using var db = new AlAnvarDBContext();
        QuranTranslations = new(await db.Translations.Where(x => x.IsActive).ToListAsync());
        QuranTranslationsACV = new AdvancedCollectionView(QuranTranslations, true);
    }
}
