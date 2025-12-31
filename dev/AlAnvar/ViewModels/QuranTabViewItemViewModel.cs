using System.Collections.ObjectModel;
using AlAnvar.Database;
using AlAnvar.Database.Tables;
using AlAnvar.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.UI.Dispatching;

namespace AlAnvar.ViewModels;

public partial class QuranTabViewItemViewModel : ObservableObject
{
    private readonly DispatcherQueue dispatcherQueue = DispatcherQueue.GetForCurrentThread();

    [ObservableProperty]
    public partial bool IsActive { get; set; }

    [ObservableProperty]
    public partial ObservableCollection<FinalQuran> Sura { get; set; }

    [ObservableProperty]
    public partial string TitleNote { get; set; }
    partial void OnTitleNoteChanged(string value)
    {
        CanSaveNote = !string.IsNullOrEmpty(value) && !string.IsNullOrEmpty(DescriptionNote);
    }

    [ObservableProperty]
    public partial string DescriptionNote { get; set; }
    partial void OnDescriptionNoteChanged(string value)
    {
        CanSaveNote = !string.IsNullOrEmpty(value) && !string.IsNullOrEmpty(TitleNote);
    }

    [ObservableProperty]
    public partial string SuraNote { get; set; }

    [ObservableProperty]
    public partial string VerseNote { get; set; }

    [ObservableProperty]
    public partial bool CanSaveNote { get; set; }
    
    public async Task OnPageLoaded(QuranMetadataTable metadataTable)
    {
        IsActive = true;

        await Task.Run(async () =>
        {
            try
            {
                using var db = new AlAnvarDBContext();
                var result = await (from q in db.Qurans
                                    where q.SuraId == metadataTable.Id
                                    join qc in db.QuransClean
                                    on new { q.SuraId, q.AyaId }
                                    equals new { qc.SuraId, qc.AyaId } into cleanJoin
                                    from qc in cleanJoin.DefaultIfEmpty()
                                    join f in db.Favorites
                                    on new { q.SuraId, q.AyaId }
                                    equals new { f.SuraId, f.AyaId } into favJoin
                                    orderby q.AyaId
                                    select new FinalQuran
                                    {
                                        Id = q.Id,
                                        SuraId = q.SuraId,
                                        AyaId = q.AyaId,
                                        Aya = q.Aya,
                                        CleanAya = qc.Aya,
                                        SuraName = metadataTable.Name,
                                        SuraFinglishName = metadataTable.FinglishName,
                                        JuzId = q.JuzId,
                                        HizbId = q.HizbId,
                                        AudioFileName = q.AudioFileName,
                                        IsFavorite = favJoin.Any()
                                    }).ToListAsync();

                var translationFile = await LoadTranslationFileAsync(Settings.Translation?.Id);

                if (translationFile != null)
                {
                    var translationDict = translationFile.Verses.ToDictionary(v => (v.SuraId, v.AyaId), v => v.Translation);

                    foreach (var verse in result)
                    {
                        translationDict.TryGetValue((verse.SuraId, verse.AyaId), out var t);
                        verse.Translation = t ?? "";
                    }
                }

                dispatcherQueue.TryEnqueue(() =>
                {
                    Sura = new ObservableCollection<FinalQuran>(result);
                });
            }
            catch (Exception ex)
            {
                IsActive = false;
                Logger?.Error(ex, ex.Message);
                await MessageBox.ShowErrorAsync(ex.Message, Strings.MessageBoxErrorTitle.GetLocalizedResource());
            }
        });

        IsActive = false;
    }
}
