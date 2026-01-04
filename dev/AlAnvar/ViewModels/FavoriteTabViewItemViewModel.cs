using System.Collections.ObjectModel;
using AlAnvar.Database;
using AlAnvar.Database.Tables;
using AlAnvar.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.UI.Dispatching;

namespace AlAnvar.ViewModels;

public partial class FavoriteTabViewItemViewModel : ObservableObject
{
    private readonly DispatcherQueue dispatcherQueue = DispatcherQueue.GetForCurrentThread();

    [ObservableProperty]
    public partial bool IsActive { get; set; }

    [ObservableProperty]
    public partial bool ShowFavoriteNotFound { get; set; }

    [ObservableProperty]
    public partial ObservableCollection<FinalQuran> Favorites { get; set; }

    public async Task OnPageLoaded(ObservableCollection<QuranMetadataTable> quranMetadata)
    {
        IsActive = true;

        await Task.Run(async () =>
        {
            try
            {
                var temp = new ObservableCollection<FinalQuran>();

                using var db = new AlAnvarDBContext();
                var plain = await db.Favorites.ToListAsync();

                foreach (var q in plain)
                {
                    var item = await db.Qurans.Where(x => x.SuraId == q.SuraId && x.AyaId == q.AyaId).FirstOrDefaultAsync();
                    var cleanItem = await db.QuransClean.Where(x => x.SuraId == q.SuraId && x.AyaId == q.AyaId).FirstOrDefaultAsync();
                    var itemChapter = quranMetadata.Where(x => x.Id == q.SuraId).FirstOrDefault();
                    temp.Add(new FinalQuran
                    {
                        Id = q.Id,
                        SuraId = q.SuraId,
                        AyaId = q.AyaId,
                        Aya = item.Aya,
                        CleanAya = cleanItem.Aya,
                        SuraName = itemChapter.Name,
                        SuraFinglishName = itemChapter.FinglishName,
                        JuzId = item.JuzId,
                        HizbId = item.HizbId,
                        AudioFileName = item.AudioFileName
                    });
                }

                var translationFile = await LoadTranslationFileAsync(Settings.Translation?.Id);

                if (translationFile != null)
                {
                    var translationDict = translationFile.Verses.ToDictionary(v => (v.SuraId, v.AyaId), v => v.Translation);

                    foreach (var verse in temp)
                    {
                        translationDict.TryGetValue((verse.SuraId, verse.AyaId), out var t);
                        verse.Translation = t ?? "";
                    }
                }

                dispatcherQueue.TryEnqueue(() =>
                {
                    Favorites = new ObservableCollection<FinalQuran>(temp.OrderByDescending(x => x.Id));

                    if (Favorites == null || Favorites.Count == 0)
                    {
                        ShowFavoriteNotFound = true;
                    }
                });
            }
            catch (Exception ex)
            {
                IsActive = false;
                Logger?.Error(ex, ex.Message);
                dispatcherQueue.TryEnqueue(async () =>
                {
                    await MessageBox.ShowErrorAsync(ex.Message, Strings.MessageBoxErrorTitle.GetLocalizedResource());
                });
            }
        });

        IsActive = false;
    }
}
