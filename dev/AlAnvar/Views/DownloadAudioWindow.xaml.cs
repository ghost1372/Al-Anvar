using System.ComponentModel;
using System.Text.RegularExpressions;
using AlAnvar.Models;
using Downloader;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Windowing;
using WinRT;

namespace AlAnvar.Views;

public sealed partial class DownloadAudioWindow : Window
{
    private readonly DispatcherQueue dispatcherQueue = DispatcherQueue.GetForCurrentThread();
    public AudioItem AudioItem { get; set; }
    private List<string> AudioUrls { get; set; }
    private DriveInfo Drive { get; set; }

    private int downloadedAudiosCount;
    private Queue<string> downloadQueue;
    private IDownload currentDownload;
    private bool isDownloading = false;
    private bool cancelRequested = false;
    public DownloadAudioWindow()
    {
        InitializeComponent();

        var glyph = GeneralHelper.GetGlyph(Strings.Main_GoToCardActionIcon.GetLocalizedResource());
        GoToCardAudio.ActionIcon = new FontIconSource { Glyph = glyph };
        Closed -= OnClosed;
        Closed += OnClosed;

        AppWindow.SetIcon("Assets/AppIcon.ico");
        ExtendsContentIntoTitleBar = true;
        SetTitleBar(AppTitleBar);

        var presenter = AppWindow.Presenter.As<OverlappedPresenter>();
        presenter.IsMaximizable = false;
    }

    private void OnClosed(object sender, WindowEventArgs args)
    {
        Cleanup();
    }
    private void Cleanup()
    {
        cancelRequested = true;
        isDownloading = false;
        currentDownload?.Stop();

        if (currentDownload != null)
        {
            currentDownload.DownloadProgressChanged -= DownloadProgressChanged;
            currentDownload.DownloadFileCompleted -= DownloadFileCompleted;
            currentDownload.DownloadStarted -= DownloadStarted;
            currentDownload = null;
        }

        downloadQueue?.Clear();
        AudioUrls?.Clear();

        dispatcherQueue?.TryEnqueue(() => { });
    }

    private async void Grid_Loaded(object sender, RoutedEventArgs e)
    {
        WindowHelper.ReActivateWindow(this);

        AppTitleBar.Title = Title = AppWindow.Title = $"{Strings.DownloadAudioWindow_DownloadAudio.GetLocalizedResource()} - {AudioItem?.Name} - {AudioItem?.PersianName}";
        AudioPathCard.Description = Path.Combine(Settings.AudiosPath, AudioItem?.DirName);

        StatusInfoBar.Title = "";
        StatusInfoBar.Message = "";
        StatusInfoBar.Severity = InfoBarSeverity.Informational;

        GetAvailableSpace();

        var result = await GetAudioPageSourceAsync(AudioItem?.Url, AudioItem?.DirName);

        if (result)
        {
            GetAvailableFiles();

            if (AudioUrls != null && AudioUrls.Count > 0)
            {
                BtnDownload.IsEnabled = true;
            }
        }
    }

    private void GetRequiredSpace(long estimatedSizeBytes)
    {
        if (AudioUrls == null)
            return;

        TxtRequiredSpace.Text = $"~ {FileHelper.GetFileSize(estimatedSizeBytes)}";

        if (estimatedSizeBytes >= Drive.AvailableFreeSpace)
        {
            RequiredSpaceStorageRing.PercentCaution = 0;

            // Disable Download Here
            StatusInfoBar.Title = Strings.DownloadAudioWindow_NoSpaceTitle.GetLocalizedResource();
            StatusInfoBar.Message = Strings.DownloadAudioWindow_NoSpaceMessage.GetLocalizedResource();
            StatusInfoBar.Severity = InfoBarSeverity.Error;
        }
        else
        {
            //Enable Download Here
            RequiredSpaceStorageRing.PercentCaution = 1001;
            StatusInfoBar.Title = Strings.DownloadAudioWindow_SufficientSpaceTitle.GetLocalizedResource();
            StatusInfoBar.Message = Strings.DownloadAudioWindow_SufficientSpaceMessage.GetLocalizedResource();
            StatusInfoBar.Severity = InfoBarSeverity.Success;
        }
    }
    private void GetAvailableFiles()
    {
        if (AudioUrls == null)
            return;

        var totalCount = AudioUrls.Count;

        var files = Directory.EnumerateFiles(Path.Combine(Settings.AudiosPath, AudioItem?.DirName), "*.mp3");
        AvailableFilesStorageRing.Value = files.Count();
        AvailableFilesStorageRing.Maximum = totalCount;
        AvailableFilesStorageRing.PercentCaution = totalCount + 1;
        AvailableFilesStorageRing.PercentCaution = totalCount + 2;

        TxtAvailableFiles.Text = $"{files.Count()}/{totalCount}";
    }
    private void GetAvailableSpace()
    {
        string rootPath = Path.GetPathRoot(Settings.AudiosPath);
        Drive = new DriveInfo(rootPath);

        long total = Drive.TotalSize;
        long free = Drive.AvailableFreeSpace;
        long used = total - free;

        TxtAvailableSpace.Text = FileHelper.GetFileSize(free);

        double percentUsed = (double)used / total * 100;
        double percentFree = (double)free / total * 100;

        AvailableSpaceStorageRing.Value = percentUsed;
    }

    private async void BtnChangeFolder_Click(object sender, RoutedEventArgs e)
    {
        var picker = new Microsoft.Windows.Storage.Pickers.FolderPicker(App.MainWindow.AppWindow.Id);
        var result = await picker.PickSingleFolderAsync();
        if (result is not null)
        {
            Settings.AudiosPath = result.Path;
            AudioPathCard.Description = result.Path;
            GetAvailableSpace();
        }
    }

    private async Task<bool> GetAudioPageSourceAsync(string url, string dirName)
    {
        if (NetworkHelper.IsNetworkAvailable())
        {
            try
            {
                using HttpClient client = new HttpClient();
                using HttpResponseMessage response = await client.GetAsync(url);
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

                var meta = GetAudioUrlsAndSizes(filePath, AudioItem?.Url);
                AudioUrls = meta.audioUrls;

                long totalSize = 0;
                var availableFiles = Directory.EnumerateFiles(Settings.AudiosPath, AudioItem?.DirName);
                foreach (var item in availableFiles)
                {
                    var removeItem = AudioUrls?.FirstOrDefault(x => x.Contains(Path.GetFileName(item)));
                    if (removeItem != null)
                    {
                        AudioUrls.Remove(removeItem);
                        FileInfo fileInfo = new FileInfo(item);
                        totalSize += fileInfo.Length;
                    }
                }

                GetRequiredSpace(meta.totalBytes - totalSize);

                StatusInfoBar.Title = Strings.DownloadAudioWindow_ReadyToDownload.GetLocalizedResource();
                StatusInfoBar.Message = "";
                StatusInfoBar.Severity = InfoBarSeverity.Success;
                return true;
            }
            catch (Exception ex)
            {
                Logger?.Error(ex, ex.Message);
                StatusInfoBar.Title = Strings.DownloadAudioWindow_Error.GetLocalizedResource();
                StatusInfoBar.Message = ex.Message;
                StatusInfoBar.Severity = InfoBarSeverity.Error;
                var result = await MessageBox.ShowErrorAsync(ex.Message, Strings.DownloadAudioWindow_Error.GetLocalizedResource(), MessageBoxButtons.RetryCancel);
                if (result == MessageBoxResult.Retry)
                {
                    Grid_Loaded(null, null);
                }

                return false;
            }
        }
        else
        {
            await MessageBox.ShowErrorAsync(Strings.DownloadAudioWindow_NoInternetMessageBoxMessage.GetLocalizedResource(), Strings.DownloadAudioWindow_NoInternetMessageBoxTitle.GetLocalizedResource());
        }

        return false;
    }
    private (List<string> audioUrls, long totalBytes) GetAudioUrlsAndSizes(string fileName, string baseUrl)
    {
        List<string> audioUrls = new List<string>();
        long totalBytes = 0;

        string content;
        using (StreamReader sr = new StreamReader(fileName))
        {
            content = sr.ReadToEnd();
        }

        if (string.IsNullOrEmpty(content))
        {
            return (null, 0);
        }

        // --- Extract mp3 links ---
        string pattern = "href=\"(.*?\\.mp3)\"";
        Regex regex = new Regex(pattern);

        foreach (Match match in regex.Matches(content))
        {
            string mp3Link = match.Groups[1].Value;
            string finalUrl = CombineUrl(baseUrl, mp3Link);
            audioUrls.Add(finalUrl);
        }

        // --- Extract file sizes (e.g., <td data-order="12345">24 KB</td>) ---
        string sizePattern = @"<td\s+data-order=""\d+"">([\d\.]+)\s*(KB|MB|GB)</td>";
        Regex sizeRegex = new Regex(sizePattern, RegexOptions.IgnoreCase);

        foreach (Match match in sizeRegex.Matches(content))
        {
            double value = double.Parse(match.Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture);
            string unit = match.Groups[2].Value.ToUpperInvariant();

            long bytes = 0;
            switch (unit)
            {
                case "KB":
                    bytes = (long)(value * 1024);
                    break;
                case "MB":
                    bytes = (long)(value * 1024 * 1024);
                    break;
                case "GB":
                    bytes = (long)(value * 1024 * 1024 * 1024);
                    break;
            }

            totalBytes += bytes;
        }

        return (audioUrls, totalBytes);
    }

    private string CombineUrl(string baseUrl, string relativeOrAbsolutePath)
    {
        Uri finalUri;

        if (Uri.TryCreate(relativeOrAbsolutePath, UriKind.Absolute, out var absUri))
        {
            // Use absolute URL but force HTTPS
            finalUri = new UriBuilder(absUri) { Scheme = Uri.UriSchemeHttps, Port = -1 }.Uri;
        }
        else
        {
            // Combine with base URL
            if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out var baseUri))
                throw new ArgumentException("Invalid base URL");

            finalUri = new Uri(baseUri, relativeOrAbsolutePath);

            // Force HTTPS
            finalUri = new UriBuilder(finalUri) { Scheme = Uri.UriSchemeHttps, Port = -1 }.Uri;
        }

        return finalUri.ToString();
    }

    private void CancelDownload()
    {
        cancelRequested = true;
        isDownloading = false;
        currentDownload?.Stop();
    }

    private void DownloadStarted(object sender, DownloadStartedEventArgs e)
    {
        dispatcherQueue.TryEnqueue(() =>
        {
            DownloadCardStackPanel.Visibility = Visibility.Visible;
        });
    }

    private void DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
    {
        dispatcherQueue.TryEnqueue(() =>
        {
            if (!downloadQueue.Any())
            {
                StatusInfoBar.Title = Strings.DownloadAudioWindow_DownloadCompleted.GetLocalizedResource();
                StatusInfoBar.Message = $"{downloadedAudiosCount}/{AudioUrls?.Count}";
                StatusInfoBar.Severity = InfoBarSeverity.Success;
                BtnChangeFolder.IsEnabled = true;
            }
        });
    }

    private void DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
    {
        dispatcherQueue.TryEnqueue(() =>
        {
            DownloadFileStorageBar.Value = e.ProgressPercentage;
            TxtStatus.Text = $"{Strings.DownloadAudioWindow_Downloading.GetLocalizedResource()} {currentDownload.Package.FileName} ({e.ProgressPercentage.ToString("0")}%)";
        });
    }

    private void StartDownloadAll()
    {
        if (AudioUrls == null || !AudioUrls.Any() || isDownloading)
            return;

        downloadQueue = new Queue<string>(AudioUrls);

        downloadedAudiosCount = 0;

        _ = ProcessDownloadQueueAsync();
    }

    private async Task ProcessDownloadQueueAsync()
    {
        isDownloading = true;
        cancelRequested = false;

        while (downloadQueue.Count > 0 && !cancelRequested)
        {
            var item = downloadQueue.Dequeue();
            DownloadFileStorageBar.Value = 0;

            currentDownload = DownloadBuilder.New()
                .WithUrl(item)
                .WithDirectory(Path.Combine(Settings.AudiosPath, AudioItem?.DirName))
                .Build();

            currentDownload.DownloadProgressChanged -= DownloadProgressChanged;
            currentDownload.DownloadProgressChanged += DownloadProgressChanged;
            currentDownload.DownloadFileCompleted -= DownloadFileCompleted;
            currentDownload.DownloadFileCompleted += DownloadFileCompleted;
            currentDownload.DownloadStarted -= DownloadStarted;
            currentDownload.DownloadStarted += DownloadStarted;

            try
            {
                await currentDownload.StartAsync();
            }
            catch (Exception ex)
            {
                Logger?.Error(ex, ex.Message);
            }

            if (cancelRequested)
                break;

            downloadedAudiosCount++;

            var percent = (double)downloadedAudiosCount / AudioUrls.Count * 100;
            TotalDownloadFileStorageBar.Value = percent;
            TxtTotalStatus.Text = $"{downloadedAudiosCount}/{AudioUrls.Count} ({percent.ToString("0")}%)";
        }

        isDownloading = false;
        currentDownload = null;
        cancelRequested = false;
        dispatcherQueue.TryEnqueue(() =>
        {
            var percent = (double)downloadedAudiosCount / AudioUrls.Count * 100;

            TxtTotalStatus.Text = $"{Strings.DownloadAudioWindow_DownloadCanceled.GetLocalizedResource()} {downloadedAudiosCount}/{AudioUrls.Count} ({percent.ToString("0")}%)";
        });
    }

    private async void Button_Click(object sender, RoutedEventArgs e)
    {
        StatusInfoBar.Title = string.Empty;
        StatusInfoBar.Message = string.Empty;
        StatusInfoBar.Severity = InfoBarSeverity.Informational;

        if (NetworkHelper.IsNetworkAvailable())
        {
            if (BtnDownload.Content.Equals(Strings.DownloadAudioWindow_StartDownload_Content.GetLocalizedResource()))
            {
                BtnChangeFolder.IsEnabled = false;
                BtnDownload.Content = Strings.DownloadAudioWindow_StartDownloadCancel.GetLocalizedResource();

                StartDownloadAll();
            }
            else
            {
                CancelDownload();
                BtnDownload.Content = Strings.DownloadAudioWindow_StartDownload_Content.GetLocalizedResource();
            }
        }
        else
        {
            await MessageBox.ShowErrorAsync(Strings.DownloadAudioWindow_NoInternetMessageBoxMessage.GetLocalizedResource(), Strings.DownloadAudioWindow_NoInternetMessageBoxTitle.GetLocalizedResource());
        }
    }
}
