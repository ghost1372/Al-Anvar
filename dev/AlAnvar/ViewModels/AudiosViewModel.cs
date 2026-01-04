using System.Collections.ObjectModel;
using AlAnvar.Database;
using AlAnvar.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.UI.Dispatching;

namespace AlAnvar.ViewModels;

public partial class AudiosViewModel : ObservableObject, ITitleBarAutoSuggestBoxAware
{
    private readonly DispatcherQueue dispatcherQueue = DispatcherQueue.GetForCurrentThread();

    private CancellationTokenSource? _token;

    [ObservableProperty]
    public partial bool IsActive { get; set; }

    [ObservableProperty]
    public partial bool ShowAudioNotFound { get; set; }

    [ObservableProperty]
    public partial string Query { get; set; }

    [ObservableProperty]
    public partial ObservableCollection<AudioItem> AudioItems { get; set; }

    [ObservableProperty]
    public partial ObservableCollection<AudioItem> LocalAudioItems { get; set; }

    public async Task DeleteSelections(List<AudioItem> audioItems)
    {
        try
        {
            foreach (var item in audioItems)
            {
                var dirPath = Path.Combine(Settings.AudiosPath, item.DirName);
                if (Directory.Exists(dirPath))
                {
                    Directory.Delete(dirPath, true);
                }
                else
                {
                    await MessageBox.ShowErrorAsync(Strings.AudiosViewModel_DirNotFoundMessageBoxMessage.GetLocalizedResource(), Strings.AudiosViewModel_DirNotFoundMessageBoxTitle.GetLocalizedResource());
                }
            }
            await MessageBox.ShowSuccessAsync(Strings.AudiosViewModel_DeleteMessageBoxSuccessMessage.GetLocalizedResource(), Strings.AudiosViewModel_DeleteMessageBoxSuccessTitle.GetLocalizedResource());
        }
        catch (Exception ex)
        {
            Logger?.Error(ex, ex.Message);
            await MessageBox.ShowErrorAsync(ex.Message, Strings.MessageBoxErrorTitle.GetLocalizedResource());
        }
    }

    public async Task GetAvailableAudios()
    {
        IsActive = true;

        await Task.Run(async() =>
        {
            try
            {
                using var db = new AlAnvarDBContext();
                var allAudios = await db.Audios
                    .Select(t => new AudioItem
                    {
                        Id = t.Id,
                        Name = t.Name,
                        PersianName = t.PersianName,
                        DirName = t.DirName,
                        Url = t.Url,
                        IsActive = false
                    })
                    .ToListAsync();

                foreach (var item in allAudios)
                {
                    var dirExist = Directory.Exists(Path.Combine(Settings.AudiosPath, item.DirName));
                    if (dirExist)
                    {
                        var fileExist = Directory.EnumerateFiles(Path.Combine(Settings.AudiosPath, item.DirName), "*.mp3").Any();
                        item.IsActive = fileExist;
                    }
                }

                dispatcherQueue.TryEnqueue(() =>
                {
                    AudioItems = new(allAudios);
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

    public async Task GetAvailableLocalAudios()
    {
        IsActive = true;

        await Task.Run(async () =>
        {
            try
            {
                using var db = new AlAnvarDBContext();
                var allAudios = await db.Audios
                    .Select(t => new AudioItem
                    {
                        Id = t.Id,
                        Name = t.Name,
                        PersianName = t.PersianName,
                        DirName = t.DirName,
                        Url = t.Url,
                        IsActive = false
                    })
                    .ToListAsync();

                foreach (var item in allAudios)
                {
                    var dirExist = Directory.Exists(Path.Combine(Settings.AudiosPath, item.DirName));
                    if (dirExist)
                    {
                        var fileExist = Directory.EnumerateFiles(Path.Combine(Settings.AudiosPath, item.DirName), "*.mp3").Any();
                        item.IsActive = fileExist;
                    }
                }

                allAudios = allAudios.Where(x => x.IsActive == true).ToList();

                dispatcherQueue.TryEnqueue(() =>
                {
                    LocalAudioItems = new(allAudios);

                    if (LocalAudioItems == null || LocalAudioItems.Count == 0)
                    {
                        ShowAudioNotFound = true;
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

    public void OnAutoSuggestBoxQuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
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

        var model = (AudioItem)item;

        return model.Name?.Contains(Query, StringComparison.OrdinalIgnoreCase) is true ||
               model.PersianName?.Contains(Query, StringComparison.OrdinalIgnoreCase) is true;
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
        AudiosPage.Instance.GetTableView().RefreshFilter();
    }
}
