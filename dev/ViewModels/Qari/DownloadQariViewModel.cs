using System.Collections.ObjectModel;

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
            var data = await db.Audios.OrderBy(x=>x.Name).ToListAsync();
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
                        var audioUrl = Path.Combine(audioItem.Url, id);
                        var audioFilePath = Path.Combine(dirPath, id);
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
                        await downloadService.DownloadFileTaskAsync(audioUrl, new DirectoryInfo(dirPath));
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
        using var streamReader = File.OpenText(fileName);
        string line = String.Empty;
        while ((line = streamReader.ReadLine()) != null)
        {
            if (line.Contains("href"))
            {
                var audioId = line.Replace("<a href=\"", "");
                var lastIndex = audioId.IndexOf("\">");
                audioId = audioId.Substring(0, lastIndex);
                quranAudioIds.Add(audioId);
            }
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
