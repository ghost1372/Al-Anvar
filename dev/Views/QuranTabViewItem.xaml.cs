﻿using System.Collections.ObjectModel;
using System.Text.RegularExpressions;

using AlAnvar.Helpers;

using Downloader;

using Microsoft.UI.Xaml.Input;

using Windows.ApplicationModel.DataTransfer;
using Windows.Media.Core;

namespace AlAnvar.Views;

public sealed partial class QuranTabViewItem : TabViewItem
{
    #region Property
    public static readonly DependencyProperty ChapterProperty =
        DependencyProperty.Register("Chapter", typeof(ChapterProperty), typeof(QuranTabViewItem),
        new PropertyMetadata(null));

    public ChapterProperty Chapter
    {
        get => (ChapterProperty) GetValue(ChapterProperty);
        set => SetValue(ChapterProperty, value);
    }

    public ObservableCollection<QuranItem> QuranCollection { get; set; } = new ObservableCollection<QuranItem>();
    private List<TranslationItem> TranslationCollection { get; set; } = new List<TranslationItem>();
    private List<Quran> AyahCollection { get; set; } = new List<Quran>();

    #region MediaPlayer
    List<AudioModel> audioList = new List<AudioModel>();
    private bool CanPlay = true;
    private DownloadService downloadService;
    private AudioModel ayaSound;
    #endregion

    public static QuranTabViewItem Instance { get; private set; }
    public QuranViewModel viewModel { get; set; }

    #endregion

    public QuranTabViewItem()
    {
        this.InitializeComponent();
        Instance = this;
        DataContext = this;
        viewModel = QuranPage.Instance.ViewModel;
        mediaPlayerElement.TransportControls.IsCompact = true;
        mediaPlayerElement.TransportControls.IsZoomButtonVisible = false;
        mediaPlayerElement.TransportControls.IsRepeatButtonVisible = true;
        mediaPlayerElement.TransportControls.IsRepeatEnabled = true;
        Loaded += QuranTabViewItem_Loaded;
        CloseRequested += QuranTabViewItem_CloseRequested;
        mediaPlayerElement.MediaPlayer.MediaEnded += MediaPlayer_MediaEnded;
    }

    private void QuranTabViewItem_CloseRequested(TabViewItem sender, TabViewTabCloseRequestedEventArgs args)
    {
        StopPlayer();
    }

    private async void QuranTabViewItem_Loaded(object sender, RoutedEventArgs e)
    {
        await Task.Run(() =>
        {
            GetSurahFromDB();
            GetTranslationText();
            GetSuraText();
            if (viewModel.CurrentQari != null)
            {
                var qariPath = Path.Combine(Settings.AudiosPath, viewModel.CurrentQari.DirName);
                GetAudios(qariPath);
            }
        });
        prgLoading.IsActive = false;
    }


    private void GetSurahFromDB()
    {
        DispatcherQueue.TryEnqueue(async () =>
        {
            AyahCollection?.Clear();
            using var db = new AlAnvarDBContext();
            AyahCollection = await db.Qurans.Where(x => x.SurahId == Chapter.Id).ToListAsync();
        });
    }

    public void GetSuraText(bool isTranslationAvailable = true)
    {
        DispatcherQueue.TryEnqueue(() =>
        {
            QuranCollection?.Clear();

            foreach (var item in AyahCollection)
            {
                QuranCollection.Add(new QuranItem
                {
                    Audio = item.Audio,
                    AyahNumber = item.AyahNumber,
                    AyahText = item.AyahText,
                    Hizb = item.Hizb,
                    Id = item.Id,
                    Juz = item.Juz,
                    SurahId = Chapter.Id,
                    SurahName = Chapter.Name,
                    TotalAyah = Chapter.Ayas,
                    AyaDetail = $"({item.AyahNumber}:{Chapter.Ayas})",
                    TranslationText = isTranslationAvailable ? TranslationCollection.Where(x => x.SurahId == Chapter.Id && x.Aya == item.AyahNumber).FirstOrDefault()?.Translation : null
                });
            }
            quranListView.ItemsSource = QuranCollection;
        });
    }

    public void GetTranslationText()
    {
        DispatcherQueue.TryEnqueue(async () =>
        {
            var currentTranslation = viewModel.CurrentTranslation;
            if (currentTranslation != null)
            {
                using var db = new AlAnvarDBContext();
                var translationItem = await db.TranslationsText.Where(x => x.TranslationId == currentTranslation.TranslationId).FirstOrDefaultAsync();
                if (translationItem != null)
                {
                    var translations = System.Text.Json.JsonSerializer.Deserialize<List<TranslationItem>>(translationItem.Text);
                    TranslationCollection = new(translations);
                    if (TranslationCollection.Count == 0)
                    {
                        viewModel.IsTranslationActive = false;
                    }
                }
            }
        });
    }

    public void GetAudios(string qariPath)
    {
        DispatcherQueue.TryEnqueue(() =>
        {
            if (Directory.Exists(qariPath))
            {
                audioList?.Clear();
                var audios = Directory.GetFiles(qariPath, "*.mp3");

                foreach (var audio in audios)
                {
                    var fileName = Path.GetFileNameWithoutExtension(audio);
                    if (Regex.IsMatch(fileName, @"^\d{6}$"))
                    {
                        var surah = Convert.ToInt32(fileName.Substring(0, 3));
                        var aya = Convert.ToInt32(fileName.Substring(3));
                        audioList.Add(new AudioModel { SurahId = surah, AyaId = aya, FileName = fileName, FullPath = audio });
                    }
                    else
                    {
                        // Skip
                        continue;
                    }
                }
            }
        });
    }

    #region ListView
    private void quranListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var item = quranListView.SelectedItem as QuranItem;
        if (item is not null)
        {
            viewModel.StatusText = $"سوره: {item.SurahName} - آیه: {item.AyahNumber}";
            UpdateMediaPlayerButtons(quranListView.SelectedIndex, quranListView.Items.Count - 1);
        }
        ScrollIntoView(quranListView.SelectedIndex);
    }

    private void quranListView_RightTapped(object sender, RightTappedRoutedEventArgs e)
    {
        menuFlyout.ShowAt(quranListView, e.GetPosition(quranListView));
    }

    public int GetListViewLastIndex()
    {
        return quranListView.Items.Count - 1;
    }

    public void GoToListViewNextItem()
    {
        var currentIndex = quranListView.SelectedIndex;
        if (currentIndex == -1)
        {
            quranListView.SelectedIndex = 0;
        }
        else if (currentIndex != quranListView.Items.Count - 1)
        {
            quranListView.SelectedIndex += 1;
        }
    }

    public int GetListViewSelectedIndex()
    {
        return quranListView.SelectedIndex;
    }

    public QuranItem GetListViewSelectedItem()
    {
        return quranListView.SelectedItem as QuranItem;
    }

    public void GoToListViewPreviousItem()
    {
        var currentIndex = quranListView.SelectedIndex;
        if (currentIndex == -1)
        {
            quranListView.SelectedIndex = 0;
        }
        else if (currentIndex != 0)
        {
            quranListView.SelectedIndex -= 1;
        }
    }

    public void ScrollIntoView(int index)
    {
        quranListView.SelectedIndex = index;
        quranListView.ScrollIntoView(quranListView.SelectedItem);
    }

    #endregion

    #region Downloader
    private void Downloader_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
    {
        DispatcherQueue.TryEnqueue(() =>
        {
            audioList.Add(ayaSound);
        });
    }

    private void Downloader_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
    {
        DispatcherQueue.TryEnqueue(() =>
        {
        });
    }

    #endregion

    #region MediaPlayer
    private void MediaPlayer_MediaEnded(Windows.Media.Playback.MediaPlayer sender, object args)
    {
        DispatcherQueue.TryEnqueue(() =>
        {
            // Go To Play Next File
            GoToListViewNextItem();

            if (CanPlay)
            {
                PlayQuran();
            }

            var currentIndex = GetListViewSelectedIndex();
            var lastIndex = GetListViewLastIndex();

            if (currentIndex == lastIndex)
            {
                SetPlayCommandState(PlayCommand.Play);
                btnPlay.IsChecked = false;
            }
        });
    }

    #region Buttons
    private void LoadMediaFromString(string path)
    {
        try
        {
            Uri pathUri = new Uri(path);
            mediaPlayerElement.Source = MediaSource.CreateFromUri(pathUri);
        }
        catch (Exception ex)
        {
            if (ex is FormatException)
            {
                // handle exception.
                // For example: Log error or notify user problem with file
            }
        }
    }

    private void StopPlayer()
    {
        mediaPlayerElement.Source = null;
        if (downloadService is not null)
        {
            downloadService.CancelAsync();
        }

        if (Temp.downloadService_Temp is not null)
        {
            Temp.downloadService_Temp?.CancelAsync();
        }
    }

    private void PreviousTrack()
    {
        StopPlayer();
        GoToListViewPreviousItem();
        PlayQuran();
    }

    private void NextTrack()
    {
        StopPlayer();
        GoToListViewNextItem();
        PlayQuran();
    }

    private async void PlayPlayer()
    {
        StopPlayer();
        var qari = viewModel.CurrentQari;
        if (qari is not null)
        {
            if (GetListViewSelectedIndex() == -1)
            {
                GoToListViewNextItem();
            }

            PlayQuran();
        }
        else
        {
            ContentDialog dialog = new ContentDialog()
            {
                Title = "قاری انتخاب نشده",
                CloseButtonText = "بستن",
                FlowDirection = FlowDirection.RightToLeft,
                Content = new ScrollViewer { Content = "به صفحه قاری ها رفته و حداقل صوت یک قاری را دانلود کنید.", Margin = new Thickness(10) },
                DefaultButton = ContentDialogButton.Primary,
                PrimaryButtonText = "دانلود صوت قاری",
                XamlRoot = App.currentWindow.Content.XamlRoot
            };

            var result = await dialog.ShowAsyncQueue();
            if (result == ContentDialogResult.Primary)
            {
                MainPage.Instance.ViewModel.JsonNavigationViewService.NavigateTo(typeof(QariPage));
            }
        }
    }

    public async void PlayQuran()
    {
        var selectedItem = GetListViewSelectedItem();
        ayaSound = audioList.Where(x => x.SurahId == selectedItem.SurahId && x.AyaId == selectedItem.AyahNumber)?.FirstOrDefault();

        if (ayaSound is null)
        {
            if (Settings.IsAutoDownloadSound && ApplicationHelper.IsNetworkAvailable())
            {
                if (downloadService is not null && downloadService.IsCancelled)
                {
                    return;
                }
                else
                {
                    if (Settings.QuranAudio != null)
                    {
                        var audioUrl = Path.Combine(Settings.QuranAudio.Url, $"{selectedItem.Audio}.mp3");
                        var dirPath = Path.Combine(Settings.AudiosPath, Settings.QuranAudio.DirName);

                        ayaSound = new AudioModel { AyaId = Convert.ToInt32(selectedItem.Audio.Substring(3)), SurahId = selectedItem.SurahId, FileName = selectedItem.Audio, FullPath = $@"{dirPath}\{selectedItem.Audio}.mp3" };
                        downloadService = new DownloadService();
                        Temp.downloadService_Temp = downloadService;
                        downloadService.DownloadFileCompleted += Downloader_DownloadFileCompleted;
                        downloadService.DownloadProgressChanged += Downloader_DownloadProgressChanged;
                        await downloadService.DownloadFileTaskAsync(audioUrl, new DirectoryInfo(dirPath));
                    }
                }
            }
            else
            {
                return;
            }
        }

        var currentIndex = GetListViewSelectedIndex();
        var lastIndex = GetListViewLastIndex();

        if (currentIndex == lastIndex)
        {
            CanPlay = false;
        }

        if (ayaSound != null)
        {
            LoadMediaFromString(ayaSound.FullPath);
            mediaPlayerElement.MediaPlayer.Play();
            SetPlayCommandState(PlayCommand.Stop);
            btnPlay.IsChecked = true;
        }
    }

    public void UpdateMediaPlayerButtons(int currentIndex, int lastIndex)
    {
        if (currentIndex == -1)
        {
            btnNext.IsEnabled = false;
            btnPrev.IsEnabled = false;
        }
        else if (currentIndex == 0)
        {
            btnNext.IsEnabled = true;
            btnPrev.IsEnabled = false;
        }
        else if (currentIndex == lastIndex)
        {
            btnNext.IsEnabled = false;
            btnPrev.IsEnabled = true;
        }
        else
        {
            btnNext.IsEnabled = true;
            btnPrev.IsEnabled = true;
        }
    }
    #endregion

    #endregion

    private void menuFlyout_Click(object sender, RoutedEventArgs e)
    {
        var selectedItem = quranListView.SelectedItem as QuranItem;
        DataPackage dataPackage = new DataPackage();
        dataPackage.RequestedOperation = DataPackageOperation.Copy;
        switch ((sender as MenuFlyoutItem).Tag)
        {
            case "Play":
                PlayPlayer();
                break;
            case "CopyTranslation":
                dataPackage.SetText(selectedItem.TranslationText);
                Clipboard.SetContent(dataPackage);
                break;
            case "CopyAya":
                dataPackage.SetText(selectedItem.AyahText);
                Clipboard.SetContent(dataPackage);
                break;
        }
    }

    private void AppBarButton_Click(object sender, RoutedEventArgs e)
    {
        var command = sender as AppBarButton;
        switch (command.Tag.ToString())
        {
            case "Previous":
                PreviousTrack();
                break;

            case "Next":
                NextTrack();
                break;
        }
    }

    private void AppBarToggleButton_Click(object sender, RoutedEventArgs e)
    {
        if (btnPlay.Label.Equals("پخش"))
        {
            SetPlayCommandState(PlayCommand.Stop);
            PlayPlayer();
        }
        else
        {
            SetPlayCommandState(PlayCommand.Play);
            StopPlayer();
        }
    }

    private void SetPlayCommandState(PlayCommand playCommand)
    {
        switch (playCommand)
        {
            case PlayCommand.Play:
                btnPlay.Label = "پخش";
                btnPlay.Icon = new SymbolIcon { Symbol = Symbol.Play };
                break;

            case PlayCommand.Stop:
                btnPlay.Label = "توقف";
                btnPlay.Icon = new SymbolIcon { Symbol = Symbol.Stop };
                break;
        }
    }

    enum PlayCommand
    {
        Play,
        Stop
    }
}