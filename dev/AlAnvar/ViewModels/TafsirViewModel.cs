using System.Collections.ObjectModel;
using AlAnvar.Database;
using AlAnvar.Database.Tables;
using AlAnvar.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.UI.Dispatching;

namespace AlAnvar.ViewModels;

public partial class TafsirViewModel : ObservableObject, ITitleBarAutoSuggestBoxAware
{
    private readonly DispatcherQueue dispatcherQueue = DispatcherQueue.GetForCurrentThread();

    [ObservableProperty]
    public partial bool IsActive { get; set; }

    [ObservableProperty]
    public partial ObservableCollection<SuraNode> Quran { get; set; }

    [ObservableProperty]
    public partial ObservableCollection<QuranTafsirNameTable> TafsirNames { get; set; }

    [ObservableProperty]
    public partial string Query { get; set; }

    public void OnAutoSuggestBoxQuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
    {
    }

    public void OnAutoSuggestBoxTextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        Query = sender.Text;
    }

    public async Task OnPageLoaded()
    {
        IsActive = true;
        await Task.Run(async() =>
        {
            try
            {
                using var db = new AlAnvarDBContext();
                var suras = await db.QuransClean.GroupBy(q => q.SuraId)
                    .Select(g => new { SuraId = g.Key, Ayas = g.OrderBy(a => a.AyaId).ToList() }).ToListAsync();

                var suraNodes = new List<SuraNode>();

                foreach (var g in suras)
                {
                    var sura = await db.Chapters.FirstOrDefaultAsync(s => s.Id == g.SuraId);

                    suraNodes.Add(new SuraNode
                    {
                        SuraId = g.SuraId,
                        SuraName = $"{sura?.Id}-{sura?.Name}",
                        Ayas = g.Ayas.Select(a => new AyaNode
                        {
                            SuraId = a.SuraId,
                            AyaId = a.AyaId,
                            AyaText = a.Aya
                        }).ToList()
                    });
                }

                var result = await db.QuranTafsirNames.ToListAsync();
                dispatcherQueue.TryEnqueue(async () =>
                {
                    TafsirNames = new(result);
                    Quran = new ObservableCollection<SuraNode>(suraNodes);
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
