
using System.Collections.ObjectModel;
using AlAnvar.Database;
using AlAnvar.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.UI.Dispatching;

namespace AlAnvar.ViewModels;

public partial class SearchViewModel : ObservableObject, ITitleBarAutoSuggestBoxAware
{
    private readonly DispatcherQueue dispatcherQueue = DispatcherQueue.GetForCurrentThread();
    private CancellationTokenSource? _token;

    [ObservableProperty]
    public partial bool IsActive { get; set; }

    [ObservableProperty]
    public partial string Query { get; set; }

    [ObservableProperty]
    public partial ObservableCollection<QuranSearchModel> Quran { get; set; }

    public async Task OnPageLoded()
    {
        IsActive = true;

        await Task.Run(async () =>
        {
            try
            {
                using var db = new AlAnvarDBContext();
                var result = await Queries.GetQuranSearchQueryAsync(db).ToListAsync();

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
                    Quran = new ObservableCollection<QuranSearchModel>(result);
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
    public async void OnAutoSuggestBoxQuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
    {
        
    }

    public async void OnAutoSuggestBoxTextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        Query = sender.Text;

        if (_token is not null)
        {
            _token.Cancel();
        }

        _token = new CancellationTokenSource();
        await RefreshFilter(_token.Token);
    }

    internal bool Filter(object? item)
    {
        if (string.IsNullOrWhiteSpace(Query)) return true;
        if (item is null) return false;

        var model = (QuranSearchModel)item;

        return model.Aya?.Contains(Query, StringComparison.OrdinalIgnoreCase) is true ||
               model.CleanAya?.Contains(Query, StringComparison.OrdinalIgnoreCase) is true ||
               model.Translation?.Contains(Query, StringComparison.OrdinalIgnoreCase) is true ||
               model.SuraName?.Contains(Query, StringComparison.OrdinalIgnoreCase) is true ||
               model.SuraFinglishName?.Contains(Query, StringComparison.OrdinalIgnoreCase) is true ||
               model.SuraEnglishName?.Contains(Query, StringComparison.OrdinalIgnoreCase) is true;
    }

    private async Task RefreshFilter(CancellationToken token)
    {
        try
        {
            await Task.Delay(200, token);
        }
        catch
        {
            return;
        }

        _token = null;
        SearchPage.Instance.GetTableView().RefreshFilter();
    }
}
