using Downloader;

namespace AlAnvar.UI.Pages;

public sealed partial class QuranPage : Page
{
    private enum PlaybackState
    {
        Playing, Stopped, Paused
    }

    private PlaybackState _playbackState;
    DispatcherTimer timer = new DispatcherTimer();
    private MediaPlayer mediaPlayer;

    List<AudioModel> audioList = new List<AudioModel>();
    private bool CanPlay = true;
    private DownloadService downloadService;
    private AudioModel ayaSound;
    internal static QuranPage Instance { get; private set; }
    public QuranPage()
    {
        this.InitializeComponent();
        Instance = this;
        _playbackState = PlaybackState.Stopped;
        timer.Interval = TimeSpan.FromSeconds(1);
        timer.Tick += Timer_Tick;
        Loaded += QuranPage_Loaded;
        
    }

    private async void QuranPage_Loaded(object sender, RoutedEventArgs e)
    {
       await Task.Run(() => {
            LoadTranslationsInCombobox();
            LoadQarisInCombobox();
        });
        prgLoading.IsActive = false;
    }

    private void Timer_Tick(object sender, object e)
    {
        UpdateSlider();
    }

    public void AddNewTab(int surahId, string name, string type, int ayaCount)
    {
        var currentTabViewItem = tabView.TabItems.Where(tabViewItem => (tabViewItem as QuranTabViewItem)?.SurahId == surahId).FirstOrDefault();
        if (currentTabViewItem is not null)
        {
            tabView.SelectedItem = currentTabViewItem;
            return;
        }

        var item = new QuranTabViewItem();
        item.Header = $"{surahId} - {name} - {ayaCount} آیه - {type}";
        item.SurahId = surahId;
        item.SurahName = name;
        item.TotalAyah = ayaCount;
        tabView.TabItems.Add(item);
        item.CloseRequested += Item_CloseRequested;
        tabView.SelectedIndex = tabView.TabItems.Count - 1;
    }

    private void Item_CloseRequested(TabViewItem sender, TabViewTabCloseRequestedEventArgs args)
    {
        tabView.TabItems.Remove(sender);
    }

    private void cmbTranslators_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        Settings.QuranTranslation = cmbTranslators.SelectedItem as QuranTranslation;

        if (GetCurrectTabViewItem() is not null)
        {
            var itemIndex = GetCurrectTabViewItem().GetListViewSelectedIndex();
            GetCurrectTabViewItem().GetTranslationText();
            GetCurrectTabViewItem().GetSuraText();
            GetCurrectTabViewItem().ScrollIntoView(itemIndex);
        }
    }

    public QuranTranslation GetComboboxFirstElement()
    {
        cmbTranslators.SelectedIndex = 0;
        return cmbTranslators.SelectedItem as QuranTranslation;
    }

    private void LoadTranslationsInCombobox()
    {
        if (Directory.Exists(Settings.TranslationsPath))
        {
            var items = new ObservableCollection<QuranTranslation>();
            var files = Directory.GetFiles(Settings.TranslationsPath, "*.ini", SearchOption.AllDirectories);
            if (files.Count() > 0)
            {
                foreach (var file in files)
                {
                    var trans = JsonConvert.DeserializeObject<QuranTranslation>(File.ReadAllText(file));
                    if (trans is not null)
                    {
                        items.Add(trans);
                    }
                }
                DispatcherQueue.TryEnqueue(() => {
                    cmbTranslators.ItemsSource = items;
                    cmbTranslators.SelectedItem = cmbTranslators.Items.Where(trans => ((QuranTranslation) trans).TranslationId == Settings.QuranTranslation?.TranslationId).FirstOrDefault();
                });
            }
        }
    }

    private void LoadQarisInCombobox()
    {
        if (Directory.Exists(Settings.AudiosPath))
        {
            var items = new ObservableCollection<QuranAudio>();
            var files = Directory.GetFiles(Settings.AudiosPath, "*.ini", SearchOption.AllDirectories);
            if (files.Count() > 0)
            {
                foreach (var file in files)
                {
                    var audio = JsonConvert.DeserializeObject<QuranAudio>(File.ReadAllText(file));
                    if (audio is not null)
                    {
                        items.Add(audio);
                    }
                }
                DispatcherQueue.TryEnqueue(() => {
                    cmbQari.ItemsSource = items;
                    cmbQari.SelectedItem = cmbQari.Items.Where(trans => ((QuranAudio) trans).DirName == Settings.QuranAudio?.DirName).FirstOrDefault();
                });
            }
        }
    }

    private void chkOnlyAyaText_Checked(object sender, RoutedEventArgs e)
    {
        if (GetCurrectTabViewItem() is not null)
        {
            GetCurrectTabViewItem().IsSurahTextAvailable = !chkOnlyAyaText.IsChecked.Value;
        }
    }

    private void chkOnlyTranslationText_Checked(object sender, RoutedEventArgs e)
    {
        if (GetCurrectTabViewItem() is not null)
        {
            GetCurrectTabViewItem().IsTranslationAvailable = !chkOnlyTranslationText.IsChecked.Value;
        }
    }

    private void cmbQari_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        audioList?.Clear();
        var qari = cmbQari.SelectedItem as QuranAudio;

        Settings.QuranAudio = qari;

        var qariPath = Path.Combine(Settings.AudiosPath, qari.DirName);
        
        if (Directory.Exists(qariPath))
        {
            var audios = Directory.GetFiles(qariPath, "*.mp3");

            foreach (var audio in audios)
            {
                var fileName = Path.GetFileNameWithoutExtension(audio);
                var surah = Convert.ToInt32(fileName.Substring(0, 3));
                var aya = Convert.ToInt32(fileName.Substring(3));
                audioList.Add(new AudioModel { SurahId = surah, AyaId = aya, FileName = fileName, FullPath = audio });
            }
        }
    }

    public void SetSurahStatus(string status)
    {
        txtStatus.Text = status;
    }

    public void CallBtnPlay()
    {
        btnPlay_Click(null, null);
    }
    private async void btnPlay_Click(object sender, RoutedEventArgs e)
    {
        var qari = cmbQari.SelectedItem as QuranAudio;
        if (qari is not null)
        {
            if (GetCurrectTabViewItem().GetListViewSelectedIndex() == -1)
            {
                GetCurrectTabViewItem().GoToListViewNextItem();
            }

            PlayQuran();
        }
        else
        {
            ContentDialog dialog = new ContentDialog()
            {
                Title = "قاری انتخاب نشده",
                CloseButtonText = "بستن",
                Content = new ScrollViewer { Content = "به صفحه قاری ها رفته و حداقل صوت یک قاری را دانلود کنید.", Margin = new Thickness(10) },
                DefaultButton = ContentDialogButton.Primary,
                PrimaryButtonText = "دانلود صوت قاری",
                XamlRoot = Content.XamlRoot
            };

            var result = await dialog.ShowAsyncQueue();
            if (result == ContentDialogResult.Primary)
            {
                ShellPage.Instance.Navigate(typeof(QariPage));
            }
        }
    }
    
    public async void PlayQuran()
    {
        var selectedItem = GetCurrectTabViewItem().GetListViewSelectedItem();
        ayaSound = audioList.Where(x => x.SurahId == selectedItem.SurahId && x.AyaId == selectedItem.AyahNumber)?.FirstOrDefault();

        if (ayaSound is null)
        {
            if (Settings.IsAutoDownloadSound && GeneralHelper.IsNetworkAvailable())
            {
                if (downloadService is not null && downloadService.IsCancelled)
                {
                    return;
                }
                else
                {
                    btnStop.IsEnabled = true;
                    btnPlay.IsEnabled = false;
                    var audioUrl = Path.Combine(Settings.QuranAudio.Url, $"{selectedItem.Audio}.mp3");
                    var dirPath = Path.Combine(Settings.AudiosPath, Settings.QuranAudio.DirName);

                    ayaSound = new AudioModel { AyaId = Convert.ToInt32(selectedItem.Audio.Substring(3)), SurahId = selectedItem.SurahId, FileName = selectedItem.Audio, FullPath = $@"{dirPath}\{selectedItem.Audio}.mp3" };
                    downloadService = new DownloadService();
                    downloadService.DownloadFileCompleted += Downloader_DownloadFileCompleted;
                    downloadService.DownloadProgressChanged += Downloader_DownloadProgressChanged;
                    await downloadService.DownloadFileTaskAsync(audioUrl, new DirectoryInfo(dirPath));
                }
            }
            else
            {
                return;
            }
        }

        btnStop.IsEnabled = true;
        btnPlay.IsEnabled = false;
        if (_playbackState == PlaybackState.Stopped)
        {
            mediaPlayer = new MediaPlayer(ayaSound.FullPath, 1);
            mediaPlayer.PlaybackPaused += MediaPlayer_PlaybackPaused;
            mediaPlayer.PlaybackResumed += MediaPlayer_PlaybackResumed;
            mediaPlayer.PlaybackStopped += MediaPlayer_PlaybackStopped;
            mediaPlayer.PlaybackStopType = MediaPlayer.PlaybackStopTypes.PlaybackStoppedReachingEndOfFile;
            mediaPlayer.TogglePlayPause(1);

            timer.Start();
            txtSoundStart.Text = GetPositionTimeFormat();
            txtSoundEnd.Text = GetLenghtTimeFormat();
            slider.Maximum = mediaPlayer.GetLenghtInSeconds();

            var currentIndex = GetCurrectTabViewItem().GetListViewSelectedIndex();
            var lastIndex = GetCurrectTabViewItem().GetListViewLastIndex();

            if (currentIndex == lastIndex)
            {
                CanPlay = false;
            }
        }
    }
    private QuranTabViewItem GetCurrectTabViewItem()
    {
        if (tabView is not null)
        {
            return tabView.SelectedItem as QuranTabViewItem;
        }
        return null;
    }
    private void Downloader_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
    {
        DispatcherQueue.TryEnqueue(() =>
        {
            audioList.Add(ayaSound);
            prgDownload.Value = 0;
        });
    }
    private void Downloader_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
    {
        DispatcherQueue.TryEnqueue(() =>
        {
            prgDownload.Value = e.ProgressPercentage;
        });
    }
    public string GetLenghtTimeFormat()
    {
        if (mediaPlayer is not null)
        {
            TimeSpan time = TimeSpan.FromSeconds(mediaPlayer.GetLenghtInSeconds());
            return time.ToString(@"hh\:mm\:ss");
        }
        return null;
    }
    public string GetPositionTimeFormat()
    {
        if (mediaPlayer is not null)
        {
            TimeSpan time = TimeSpan.FromSeconds(mediaPlayer.GetPositionInSeconds());
            return time.ToString(@"hh\:mm\:ss");
        }
        return null;
    }

    public void UpdateMediaPlayerButtons(int currentIndex, int lastIndex)
    {
        if (currentIndex == -1)
        {
            btnNext.IsEnabled = false;
            btnPrevious.IsEnabled = false;
        }
        else if (currentIndex == 0)
        {
            btnNext.IsEnabled = true;
            btnPrevious.IsEnabled = false;
        }
        else if (currentIndex == lastIndex)
        {
            btnNext.IsEnabled = false;
            btnPrevious.IsEnabled = true;
        }
        else
        {
            btnNext.IsEnabled = true;
            btnPrevious.IsEnabled = true;
        }
    }

    #region MediaPlayer Events
    private void MediaPlayer_PlaybackStopped()
    {
        _playbackState = PlaybackState.Stopped;
        slider.Value = 0;
        txtSoundStart.Text = "00:00:00";
        txtSoundEnd.Text = "00:00:00";
        btnStop.IsEnabled = false;
        btnPlay.IsEnabled = true;
        if (mediaPlayer.PlaybackStopType == MediaPlayer.PlaybackStopTypes.PlaybackStoppedReachingEndOfFile)
        {
            // Go To Play Next File
            if (!chkRepeat.IsChecked.Value)
            {
                GetCurrectTabViewItem().GoToListViewNextItem();
            }
            else
            {
                CanPlay = true;
            }
            
            if (CanPlay)
            {
                PlayQuran();
            }
        }
    }

    private void MediaPlayer_PlaybackResumed()
    {
        _playbackState = PlaybackState.Playing;
        timer.Start();
    }
    private void MediaPlayer_PlaybackPaused()
    {
        _playbackState = PlaybackState.Paused;
        timer.Stop();
    }
    #endregion

    private void btnStop_Click(object sender, RoutedEventArgs e)
    {
        if (mediaPlayer != null)
        {
            timer.Stop();
            mediaPlayer.PlaybackStopType = MediaPlayer.PlaybackStopTypes.PlaybackStoppedByUser;
            mediaPlayer.Stop();
            if (downloadService is not null)
            {
                downloadService.CancelAsync();
            }
        }
    }
    private void btnPrevious_Click(object sender, RoutedEventArgs e)
    {
        btnStop_Click(null, null);
        GetCurrectTabViewItem().GoToListViewPreviousItem();
        PlayQuran();
    }

    private void btnNext_Click(object sender, RoutedEventArgs e)
    {
        btnStop_Click(null, null);
        GetCurrectTabViewItem().GoToListViewNextItem();
        PlayQuran();
    }

    private void slider_ManipulationStarted(object sender, Microsoft.UI.Xaml.Input.ManipulationStartedRoutedEventArgs e)
    {
        if (mediaPlayer != null)
        {
            mediaPlayer.Pause();

            timer.Stop();
        }
    }

    private void slider_ManipulationCompleted(object sender, Microsoft.UI.Xaml.Input.ManipulationCompletedRoutedEventArgs e)
    {
        if (mediaPlayer != null)
        {
            timer.Start();

            mediaPlayer.SetPosition(slider.Value);
            mediaPlayer.Play(NAudio.Wave.PlaybackState.Paused, 1);
        }
    }

    private void UpdateSlider()
    {
        if (_playbackState == PlaybackState.Playing)
        {
            slider.Value = mediaPlayer.GetPositionInSeconds();
            txtSoundStart.Text = GetPositionTimeFormat();
        }
    }

    private void mnuToolbar_Click(object sender, RoutedEventArgs e)
    {
        switch ((sender as AppBarButton).Tag)
        {
            case "GoToFont":
                ShellPage.Instance.Navigate(typeof(SettingsPage));
                break;
            case "AddNote":

                break;
            case "Subject":

                break;
            case "Tafsir":

                break;
        }
    }

    private void tabView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        switch (tabView.TabItems.Count)
        {
            case 0:
                toolbarGrid.Visibility = Visibility.Collapsed;
                ShellPage.Instance.DeSelectListView();
                break;
            default:
                toolbarGrid.Visibility = Visibility.Visible;
                break;
        }
    }
}
