using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using ColorCode.Compilation.Languages;
using Downloader;
using Newtonsoft.Json;

namespace AlAnvar.ViewModels;
public partial class DownloadQariViewModel : ObservableRecipient, ITitleBarAutoSuggestBoxAware
{
    private readonly DispatcherQueue dispatcherQueue = DispatcherQueue.GetForCurrentThread();

    [ObservableProperty]
    private ObservableCollection<QuranAudio> quranAudios;

    [ObservableProperty]
    public AdvancedCollectionView quranAudiosACV;

    [ObservableProperty]
    public bool isDownloadActive = true;

    [ObservableProperty]
    public bool isCancelActive;

    [ObservableProperty]
    public double progressValue;

    [ObservableProperty]
    public double progressValue2;

    [ObservableProperty]
    public int progressMax;

    [ObservableProperty]
    public string statusText;

    [ObservableProperty]
    public object listViewSelectedItem;

    private int _totalDownloadItemCount = 0;
    private int _downloadedtItemCount = 0;

    private DownloadService downloadService;
    public List<string> autoSuggestBoxSuggestList = new List<string>();

    [RelayCommand]
    private async void OnPageLoaded()
    {
        IsActive = true;
        IsDownloadActive = false;
        await Task.Run(async () =>
        {
            using var db = new AlAnvarDBContext();
            var data = await db.Audios.OrderBy(x => x.Name).ToListAsync();
            QuranAudios = new(data);
            dispatcherQueue.TryEnqueue(() =>
            {
                QuranAudiosACV = new AdvancedCollectionView(QuranAudios, true);
                autoSuggestBoxSuggestList = QuranAudiosACV.Select(x => ((QuranAudio) x).PName).ToList();
            });
        });
        IsActive = false;
        IsDownloadActive = true;
    }

    public void Search(AutoSuggestBox sender)
    {
        if (QuranAudiosACV != null)
        {
            QuranAudiosACV.Filter = _ => true;
            QuranAudiosACV.Filter = audio =>
            {
                var query = audio as QuranAudio;

                var name = query.Name ?? "";
                var pName = query.PName ?? "";

                return name.Contains(sender.Text, StringComparison.OrdinalIgnoreCase)
                    || pName.Contains(sender.Text, StringComparison.OrdinalIgnoreCase);
            };
        }
    }

    [RelayCommand]
    private void OnListViewItemChanged()
    {
        IsDownloadActive = ListViewSelectedItem != null;
    }

    [RelayCommand]
    private void OnDownloadQari()
    {
        IsDownloadActive = false;
        IsCancelActive = true;
        DownloadQariAsync();
    }

    [RelayCommand]
    private void OnCancelDownlaod()
    {
        IsDownloadActive = true;
        IsCancelActive = false;
        CancelDownload();
    }

    private void CancelDownload()
    {
        downloadService?.CancelAsync();
    }

    private async void DownloadQariAsync()
    {
        ProgressValue = 0;
        ProgressValue2 = 0;
        StatusText = "0/0";
        _downloadedtItemCount = 0;
        downloadService = null;

        try
        {
            if (ListViewSelectedItem != null)
            {
                var audioItem = ListViewSelectedItem as QuranAudio;
                var sourceFilePath = await GetAudioPageSourceAsync(audioItem.Url, audioItem.DirName);

                var audioIds = GetAudioIds(sourceFilePath).Where(x => x.Contains(".mp3"));
                var dirPath = Path.Combine(Settings.AudiosPath, audioItem.DirName);
                ProgressMax = _totalDownloadItemCount = audioIds.Count();

                var content = JsonConvert.SerializeObject(audioItem, Formatting.Indented);
                File.WriteAllText($@"{dirPath}\{audioItem.DirName}.ini", content);

                foreach (var id in audioIds)
                {
                    if (downloadService is not null && downloadService.IsCancelled)
                    {
                        return;
                    }
                    else
                    {
                        var audioUrlPath = id;
                        if (audioItem.Url.EndsWith("/") && audioUrlPath.StartsWith("/"))
                        {
                            audioUrlPath = audioUrlPath.Remove(0, 1);
                        }
                        else if (!audioItem.Url.EndsWith("/") && !audioUrlPath.StartsWith("/"))
                        {
                            audioUrlPath = audioUrlPath.Insert(0, "/");
                        }

                        var finalAudioUrlPath = Path.Combine(audioItem.Url, audioUrlPath);
                        var audioFilePath = Path.Combine(dirPath, finalAudioUrlPath);
                        if (File.Exists(audioFilePath))
                        {
                            _downloadedtItemCount += 1;
                            ProgressValue2 += 1;
                            StatusText = $"{_downloadedtItemCount}/{_totalDownloadItemCount}";
                            continue;
                        }
                        downloadService = new DownloadService();
                        downloadService.DownloadProgressChanged += Downloader_DownloadProgressChanged;
                        downloadService.DownloadFileCompleted += Downloader_DownloadFileCompleted;
                        await downloadService.DownloadFileTaskAsync(finalAudioUrlPath, new DirectoryInfo(dirPath));
                    }
                }
            }
            else
            {
                IsDownloadActive = true;
                IsCancelActive = false;
            }
        }
        catch (Exception)
        {
            IsDownloadActive = true;
            IsCancelActive = false;
        }
    }
    private void Downloader_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
    {
        dispatcherQueue.TryEnqueue(() =>
        {
            _downloadedtItemCount += 1;
            ProgressValue2 += 1;
            StatusText = $"{_downloadedtItemCount}/{_totalDownloadItemCount}";
            if (_downloadedtItemCount == _totalDownloadItemCount)
            {
                IsDownloadActive = true;
                IsCancelActive = false;
            }
        });
    }

    private void Downloader_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
    {
        dispatcherQueue.TryEnqueue(() =>
        {
            ProgressValue = e.ProgressPercentage;
        });
    }

    private List<string> GetAudioIds(string fileName)
    {
        List<string> quranAudioIds = new List<string>();

        string content;
        using (StreamReader sr = new StreamReader(fileName))
        {
            content = sr.ReadToEnd();
        }

        if (string.IsNullOrEmpty(content))
        {
            return null;
        }

        string pattern = "href=\"(.*?\\.mp3)\"";
        Regex regex = new Regex(pattern);

        foreach (Match match in regex.Matches(content))
        {
            string mp3Link = match.Groups[1].Value;
            mp3Link = Path.GetFileName(mp3Link);
            quranAudioIds.Add(mp3Link);
        }

        return quranAudioIds;
    }

    private async Task<string> GetAudioPageSourceAsync(string qariUrl, string dirName)
    {
        using HttpClient client = new HttpClient();
        using HttpResponseMessage response = await client.GetAsync(qariUrl);
        response.EnsureSuccessStatusCode();
        using HttpContent content = response.Content;
        string result = await content.ReadAsStringAsync();

        var audioPath = Path.Combine(Settings.AudiosPath, dirName);
        if (!Directory.Exists(audioPath))
        {
            Directory.CreateDirectory(audioPath);
        }

        var filePath = Path.Combine(audioPath, $"{dirName}.txt");
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
        using (var outfile = new StreamWriter(filePath))
        {
            outfile.WriteLine(result);
        }

        return filePath;
    }

    public void OnAutoSuggestBoxTextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        Search(sender);
    }

    public void OnAutoSuggestBoxQuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
    {
        Search(sender);
    }
}
