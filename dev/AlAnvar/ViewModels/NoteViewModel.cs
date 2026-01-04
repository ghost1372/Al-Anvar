using System.Collections.ObjectModel;
using AlAnvar.Database;
using AlAnvar.Database.Tables;
using AlAnvar.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.UI.Dispatching;

namespace AlAnvar.ViewModels;

public partial class NoteViewModel : ObservableObject
{
    private readonly DispatcherQueue dispatcherQueue = DispatcherQueue.GetForCurrentThread();

    [ObservableProperty]
    public partial bool IsActive { get; set; }

    [ObservableProperty]
    public partial bool ShowNoteNotFound { get; set; }

    [ObservableProperty]
    public partial ObservableCollection<SuraNode> Notes { get; set; }

    [ObservableProperty]
    public partial ObservableCollection<QuranMetadataTable> QuranMetaItems { get; set; }

    [ObservableProperty]
    public partial ObservableCollection<FinalQuran> QuranVerses { get; set; }

    public async Task GetQuranVerse(QuranMetadataTable quranMetadataTable)
    {
        IsActive = true;
        await Task.Run(async() =>
        {
            try
            {
                using var db = new AlAnvarDBContext();
                var result = await Queries.GetQuranWithCleanBySuraQueryAsync(db, quranMetadataTable.Id).ToListAsync();
                foreach (var item in result)
                {
                    item.SuraName = quranMetadataTable.Name;
                    item.SuraFinglishName = quranMetadataTable.FinglishName;
                }
               
                dispatcherQueue.TryEnqueue(() =>
                {
                    QuranVerses = new(result);
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

    public async Task OnPageLoaded()
    {
        IsActive = true;

        await Task.Run(async() =>
        {
            try
            {
                using var db = new AlAnvarDBContext();
                var notes = await Queries.GetAllNotesQueryAsync(db).ToListAsync();
                var chapters = await Queries.GetAllChaptersQueryAsync(db).ToListAsync();
                var quran = await Queries.GetQuranCleanQueryAsync(db).ToListAsync();
                var result = notes.GroupBy(n => n.SuraId).Select(suraGroup =>
                {
                    var suraId = suraGroup.Key;

                    var suraName = chapters.Where(c => c.Id == suraId).Select(c => c.Name).FirstOrDefault();

                    return new SuraNode
                    {
                        SuraId = suraId,
                        SuraName = suraName,

                        Ayas = suraGroup.GroupBy(n => n.AyaId).Select(ayaGroup =>
                        {
                            var ayaId = ayaGroup.Key;

                            var ayaText = quran.Where(q => q.SuraId == suraId && q.AyaId == ayaId).Select(q => q.Aya).FirstOrDefault();

                            return new AyaNode
                            {
                                AyaId = ayaId,
                                SuraId = suraId,
                                AyaText = ayaText,

                                Notes = ayaGroup.Select(n => new NoteNode
                                {
                                    NoteId = n.Id,
                                    SuraId = n.SuraId,
                                    AyaId = n.AyaId,
                                    Title = n.Title,
                                    Description = n.Description,
                                    CreatedAt = n.CreatedAt,
                                    UpdatedAt = n.UpdatedAt
                                }).ToList()
                            };
                        }).OrderBy(a => a.AyaId).ToList()
                    };
                }).OrderBy(s => s.SuraId).ToList();

                dispatcherQueue.TryEnqueue(() =>
                {
                    QuranMetaItems = new ObservableCollection<QuranMetadataTable>(chapters);
                    Notes = new ObservableCollection<SuraNode>(result);

                    if (Notes == null || Notes.Count == 0)
                    {
                        ShowNoteNotFound = true;
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
