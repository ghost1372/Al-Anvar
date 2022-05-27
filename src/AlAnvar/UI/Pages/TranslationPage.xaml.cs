using Downloader;

namespace AlAnvar.UI.Pages;

public sealed partial class TranslationPage : Page
{
    public ObservableCollection<QuranTranslation> QuranTranslations { get; set; }
    public ObservableCollection<QuranTranslation> LocalTranslations { get; set; }
    private List<string> InlineQuranTranslationSuggestions { get; } = new();

    public AdvancedCollectionView QuranTranslationsACV;

    private int _totalDownloadItemCount = 0;
    private int _downloadedtItemCount = 0;
    public TranslationPage()
    {
        this.InitializeComponent();
        Loaded += TranslationPage_Loaded;
    }

    private void TranslationPage_Loaded(object sender, RoutedEventArgs e)
    {
        GetQuranTranslations();

        GetLocalTranslations();
    }

    private async void GetQuranTranslations()
    {
        using var db = new AlAnvarDBContext();
        var data = await db.Translations.ToListAsync();

        //Remove Already Exist Translation from Download List
        if (Directory.Exists(Settings.TranslationsPath))
        {
            var localFiles = Directory.GetFiles(Settings.TranslationsPath, "*.ini", SearchOption.AllDirectories);
            if (localFiles.Count() > 0)
            {
                foreach (var item in localFiles)
                {
                    var translationItem = JsonConvert.DeserializeObject<QuranTranslation>(File.ReadAllText(item));
                    if (translationItem is not null)
                    {
                        data.Remove(translationItem);
                    }
                }
            }
        }

        QuranTranslations = new(data);
        QuranTranslationsACV = new AdvancedCollectionView(QuranTranslations, true);
        rootListView.ItemsSource = QuranTranslationsACV;

        InlineQuranTranslationSuggestions?.Clear();

        //Fill InlineAutoCompleteTextBox
        foreach (var item in QuranTranslations)
        {
            InlineQuranTranslationSuggestions.Add(item.Name);
            InlineQuranTranslationSuggestions.Add(item.Language);
            InlineQuranTranslationSuggestions.Add(item.Translator);
        }
    }
    private async void btnDownload_Click(object sender, RoutedEventArgs e)
    {
        var selectedItems = rootListView.SelectedItems;
        btnDownload.IsEnabled = false;
        btnRefresh.IsEnabled = false;
        rootListView.IsEnabled = false;
        try
        {
            if (selectedItems.Count > 0)
            {
                _totalDownloadItemCount = selectedItems.Count;
                _downloadedtItemCount = 0;
                foreach (var item in selectedItems)
                {
                    var translation = item as QuranTranslation;
                    var dirInfo = Path.Combine(Settings.TranslationsPath, translation.TranslationId);
                    DirectoryInfo path = new DirectoryInfo(dirInfo);
                    if (!Directory.Exists(dirInfo))
                    {
                        Directory.CreateDirectory(dirInfo);
                        string jsonString = JsonConvert.SerializeObject(translation);
                        File.WriteAllText(Path.Combine(dirInfo, $"{translation.TranslationId}.ini"), jsonString);
                    }
                    var downloader = new DownloadService();
                    downloader.DownloadProgressChanged += Downloader_DownloadProgressChanged;
                    downloader.DownloadFileCompleted += Downloader_DownloadFileCompleted;
                    await downloader.DownloadFileTaskAsync(translation.Link, path);
                }
            }
        }
        catch (Exception)
        {
            btnDownload.IsEnabled = true;
            btnRefresh.IsEnabled = true;
            rootListView.IsEnabled = true;
        }
    }

    private void Downloader_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
    {
        DispatcherQueue.TryEnqueue(() =>
        {
            _downloadedtItemCount += 1;
            txtStatus.Text = $"{_downloadedtItemCount}/{_totalDownloadItemCount}";
            if (_downloadedtItemCount == _totalDownloadItemCount)
            {
                btnDownload.IsEnabled = true;
                btnRefresh.IsEnabled = true;
                rootListView.IsEnabled = true;
                rootListView.SelectedItems.Clear();
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

    private void rootListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        btnDownload.IsEnabled = rootListView.SelectedItems.Count > 0;
    }

    private void txtInlineSearch_TextChanged(object sender, TextChangedEventArgs e)
    {
        QuranTranslationsACV.Filter = _ => true;
        QuranTranslationsACV.Filter = TranslationsFilter;
    }
    private bool TranslationsFilter(object translate)
    {
        var query = translate as QuranTranslation;

        var name = query.Name ?? "";
        var language = query.Language ?? "";
        var translator = query.Translator ?? "";

        return name.Contains(txtInlineSearch.Text, StringComparison.OrdinalIgnoreCase)
                || language.Contains(txtInlineSearch.Text, StringComparison.OrdinalIgnoreCase)
                || translator.Contains(txtInlineSearch.Text, StringComparison.OrdinalIgnoreCase);
    }

    private void btnRefresh_Click(object sender, RoutedEventArgs e)
    {
        GetQuranTranslations();
    }

    #region Local Translations
    private void GetLocalTranslations()
    {
        if (Directory.Exists(Settings.TranslationsPath))
        {
            LocalTranslations = new();
            var localFiles = Directory.GetFiles(Settings.TranslationsPath, "*.ini", SearchOption.AllDirectories);
            if (localFiles.Count() > 0)
            {
                foreach (var file in localFiles)
                {
                    var item = JsonConvert.DeserializeObject<QuranTranslation>(File.ReadAllText(file));
                    LocalTranslations.Add(item);
                }
                localListView.ItemsSource = LocalTranslations;
            }
        }
    }
    private void localListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        btnDelete.IsEnabled = localListView.SelectedItems.Count > 0;
    }

    private void btnDelete_Click(object sender, RoutedEventArgs e)
    {
        if (localListView.SelectedItems.Count > 0)
        {
            foreach (var item in localListView.SelectedItems)
            {
                var translationId = (item as QuranTranslation).TranslationId;
                var dirPath = Path.Combine(Settings.TranslationsPath, translationId);
                Directory.Delete(dirPath, true);
            }
            GetLocalTranslations();
        }
    }
    private void btnRefreshLocal_Click(object sender, RoutedEventArgs e)
    {
        GetLocalTranslations();
    }
    #endregion
}
