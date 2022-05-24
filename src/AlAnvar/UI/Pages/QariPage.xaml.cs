using Downloader;

namespace AlAnvar.UI.Pages;
public sealed partial class QariPage : Page
{
    public ObservableCollection<QuranAudio> QuranAudios { get; set; }
    public ObservableCollection<QuranAudio> LocalAudios { get; set; }
    private List<string> InlineQuranAudioSuggestions { get; } = new();

    public AdvancedCollectionView QuranAudiosACV;
    private int _totalDownloadItemCount = 0;
    private int _downloadedtItemCount = 0;

    private DownloadService downloadService;
    public QariPage()
    {
        this.InitializeComponent();
        Loaded += QariPage_Loaded;
    }

    private void QariPage_Loaded(object sender, RoutedEventArgs e)
    {
        GetQuranAudios();

        GetLocalAudios();
    }

    private async void GetQuranAudios()
    {
        using var db = new AlAnvarDBContext();
        var data = await db.Audios.ToListAsync();

        QuranAudios = new(data);
        QuranAudiosACV = new AdvancedCollectionView(QuranAudios, true);
        rootListView.ItemsSource = QuranAudiosACV;

        InlineQuranAudioSuggestions?.Clear();
        //Fill InlineAutoCompleteTextBox
        foreach (var item in QuranAudios)
        {
            InlineQuranAudioSuggestions.Add(item.Name);
            InlineQuranAudioSuggestions.Add(item.PName);
        }
    }
    private void txtInlineSearch_TextChanged(object sender, TextChangedEventArgs e)
    {
        QuranAudiosACV.Filter = _ => true;
        QuranAudiosACV.Filter = AudiosFilter;
    }
    private bool AudiosFilter(object audio)
    {
        var query = audio as QuranAudio;

        var name = query.Name ?? "";
        var pName = query.PName ?? "";

        return name.Contains(txtInlineSearch.Text, StringComparison.OrdinalIgnoreCase)
                || pName.Contains(txtInlineSearch.Text, StringComparison.OrdinalIgnoreCase);
    }
    private void rootListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        btnDownload.IsEnabled = rootListView.SelectedItems.Count > 0;
    }

    private async void btnDownload_Click(object sender, RoutedEventArgs e)
    {
        btnCancel.IsEnabled = true;
        btnDownload.IsEnabled = false;
        btnRefresh.IsEnabled = false;
        rootListView.IsEnabled = false;
        prgStatus.Value = 0;
        prgStatus2.Value = 0;
        txtStatus.Text = "0/0";
        _downloadedtItemCount = 0;
        downloadService = null;

        try
        {
            var audioItem = (rootListView.SelectedItem as QuranAudio);
            var sourceFilePath = await GetAudioPageSource(audioItem.Url, audioItem.DirName);

            var audioIds = GetAudioIds(sourceFilePath).Where(x => x.Contains(".mp3"));
            var dirPath = Path.Combine(Constants.AudiosPath, audioItem.DirName);
            prgStatus2.Maximum = _totalDownloadItemCount = audioIds.Count();

            var json = JsonConvert.SerializeObject(audioItem);

            File.WriteAllText($@"{dirPath}\{audioItem.DirName}.ini", json);

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
                        prgStatus2.Value += 1;
                        txtStatus.Text = $"{_downloadedtItemCount}/{_totalDownloadItemCount}";
                        continue;
                    }
                    downloadService = new DownloadService();
                    downloadService.DownloadProgressChanged += Downloader_DownloadProgressChanged;
                    downloadService.DownloadFileCompleted += Downloader_DownloadFileCompleted;
                    await downloadService.DownloadFileTaskAsync(audioUrl, new DirectoryInfo(dirPath));
                }
            }
        }
        catch (Exception)
        {
            btnDownload.IsEnabled = true;
            btnRefresh.IsEnabled = true;
            rootListView.IsEnabled = true;
            btnCancel.IsEnabled = false;
        }
    }
    private void Downloader_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
    {
        DispatcherQueue.TryEnqueue(() =>
        {
            _downloadedtItemCount += 1;
            prgStatus2.Value += 1;
            txtStatus.Text = $"{_downloadedtItemCount}/{_totalDownloadItemCount}";
            if (_downloadedtItemCount == _totalDownloadItemCount)
            {
                btnDownload.IsEnabled = true;
                btnRefresh.IsEnabled = true;
                rootListView.IsEnabled = true;
                btnCancel.IsEnabled = false;
            }
        });
    }
    private void Downloader_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
    {
        DispatcherQueue.TryEnqueue(() =>
        {
            prgStatus.Value = e.ProgressPercentage;
        });
    }
    private List<string> GetAudioIds(string fileName)
    {
        List<string> quranAudioIds = new List<string>();
        using (var streamReader = File.OpenText(fileName))
        {
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
        }
        return quranAudioIds;
    }
    private async Task<string> GetAudioPageSource(string qariUrl, string dirName)
    {
        using HttpClient client = new HttpClient();
        using HttpResponseMessage response = await client.GetAsync(qariUrl);
        response.EnsureSuccessStatusCode();
        using HttpContent content = response.Content;
        string result = await content.ReadAsStringAsync();

        var audioPath = Path.Combine(Constants.AudiosPath, dirName);
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
    private void btnRefresh_Click(object sender, RoutedEventArgs e)
    {
        GetQuranAudios();
    }

    private void btnCancel_Click(object sender, RoutedEventArgs e)
    {
        downloadService.CancelAsync();
        btnDownload.IsEnabled = true;
        btnRefresh.IsEnabled = true;
        rootListView.IsEnabled = true;
        btnCancel.IsEnabled = false;
    }
    #region Local Translations
    private void localListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        btnDelete.IsEnabled = localListView.SelectedItems.Count > 0;
    }

    private void btnRefreshLocal_Click(object sender, RoutedEventArgs e)
    {
        GetLocalAudios();
    }

    private void btnDelete_Click(object sender, RoutedEventArgs e)
    {
        if (localListView.SelectedItems.Count > 0)
        {
            foreach (var item in localListView.SelectedItems)
            {
                var audioId = (item as QuranAudio).DirName;
                var dirPath = Path.Combine(Constants.AudiosPath, audioId);
                Directory.Delete(dirPath, true);
            }
            GetLocalAudios();
        }
    }

    private void GetLocalAudios()
    {
        if (Directory.Exists(Constants.AudiosPath))
        {
            LocalAudios = new();
            var localFiles = Directory.GetFiles(Constants.AudiosPath, "*.ini", SearchOption.AllDirectories);
            if (localFiles.Count() > 0)
            {
                foreach (var file in localFiles)
                {
                    var item = JsonConvert.DeserializeObject<QuranAudio>(File.ReadAllText(file));
                    LocalAudios.Add(item);
                }
                localListView.ItemsSource = LocalAudios;
            }
        }
    }
    #endregion
}
