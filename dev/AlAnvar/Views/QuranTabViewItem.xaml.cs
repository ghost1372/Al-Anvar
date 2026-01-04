using System.ComponentModel;
using System.Text;
using AlAnvar.Database;
using AlAnvar.Database.Tables;
using AlAnvar.Models;
using Downloader;
using Microsoft.EntityFrameworkCore;
using Microsoft.UI.Composition;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.Windows.Storage.Pickers;
using Windows.Media.Core;
using WinUI.TableView;

namespace AlAnvar.Views;

public sealed partial class QuranTabViewItem : TabViewItem
{
    private readonly DispatcherQueue dispatcherQueue = DispatcherQueue.GetForCurrentThread();

    private IDownload download;
    public QuranMetadataTable Metadata
    {
        get { return (QuranMetadataTable)GetValue(MetadataProperty); }
        set { SetValue(MetadataProperty, value); }
    }

    public static readonly DependencyProperty MetadataProperty =
        DependencyProperty.Register(nameof(Metadata), typeof(QuranMetadataTable), typeof(QuranTabViewItem), new PropertyMetadata(null));

    public QuranViewModel QuranViewModel
    {
        get { return (QuranViewModel)GetValue(QuranViewModelProperty); }
        set { SetValue(QuranViewModelProperty, value); }
    }

    public static readonly DependencyProperty QuranViewModelProperty =
        DependencyProperty.Register(nameof(QuranViewModel), typeof(QuranViewModel), typeof(QuranTabViewItem), new PropertyMetadata(null));

    public int VerseSelectedIndex
    {
        get { return (int)GetValue(VerseSelectedIndexProperty); }
        set { SetValue(VerseSelectedIndexProperty, value); }
    }

    public static readonly DependencyProperty VerseSelectedIndexProperty =
        DependencyProperty.Register(nameof(VerseSelectedIndex), typeof(int), typeof(QuranTabViewItem), new PropertyMetadata(-1));

    public QuranTabViewItemViewModel ViewModel { get; }

    internal static QuranTabViewItem Instance { get; private set; }
    private Visual audioPlayerHostVisual;
    private int _currentIndex = -1;
    private bool isAnyFilePlayed = false;

    private int noteSuraId;
    private int noteVerseId;
    public QuranTabViewItem()
    {
        ViewModel = App.GetService<QuranTabViewItemViewModel>();
        InitializeComponent();

        Instance = this;

        audioPlayerHostVisual = InitTranslationAnimation(AudioPlayerHost);
        AudioPlayer.TransportControls.IsCompact = true;
        AudioPlayer.TransportControls.IsZoomButtonVisible = false;
        AudioPlayer.TransportControls.IsRepeatButtonVisible = true;
        AudioPlayer.TransportControls.IsRepeatEnabled = true;
        AudioPlayer.MediaPlayer.CurrentStateChanged -= MediaPlayer_PlaybackStateChanged;
        AudioPlayer.MediaPlayer.CurrentStateChanged += MediaPlayer_PlaybackStateChanged;

        Loaded -= OnLoaded;
        Loaded += OnLoaded;

        CloseRequested -= QuranTabViewItem_CloseRequested;
        CloseRequested += QuranTabViewItem_CloseRequested;
    }

    private void QuranTabViewItem_CloseRequested(TabViewItem sender, TabViewTabCloseRequestedEventArgs args)
    {
        DisposeAudio();
    }

    private void MediaPlayer_PlaybackStateChanged(Windows.Media.Playback.MediaPlayer sender, object args)
    {
        if (sender.PlaybackSession.PlaybackState == Windows.Media.Playback.MediaPlaybackState.Playing)
        {
            isAnyFilePlayed = true;
            AudioPlayer.MediaPlayer.CurrentStateChanged -= MediaPlayer_PlaybackStateChanged;
        }
    }
    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        QuranTableView.SortDescriptions.Add(item: new SortDescription("AyaId", SortDirection.Ascending));
        QuranTableView.FilterDescriptions.Add(new FilterDescription(string.Empty, QuranViewModel.Filter));

        await ViewModel.OnPageLoaded(Metadata);

        GoToAya(VerseSelectedIndex);
    }

    public void GoToAya(int index)
    {
        index = index - 1;
        var count = QuranTableView.Items.Count;
        if (index >= count || index < 0)
            return;

        SetSelectedIndex(index);
    }
    private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (QuranTableView.SelectedIndex != -1)
        {
            _currentIndex = QuranTableView.SelectedIndex;

            if (Settings.Audio == null)
                return;

            SetAudioPlayerHostVisibility(Visibility.Visible);

            if (QuranTableView.SelectedItem is FinalQuran quran)
            {
                SetAudioSource(Path.Combine(Settings.AudiosPath, Settings.Audio.DirName, $"{quran.AudioFileName}.mp3"));
            }
        }
        else
        {
            SetAudioPlayerHostVisibility(Visibility.Collapsed);
        }
    }
    private void GoToNextItem()
    {
        if (QuranTableView.Items.Count == 0)
            return;

        int nextIndex = _currentIndex + 1;

        if (nextIndex >= QuranTableView.Items.Count)
            return;

        SetSelectedIndex(nextIndex);
    }
    private void SetSelectedIndex(int index)
    {
        DispatcherQueue.TryEnqueue(() =>
        {
            QuranTableView.SelectedIndex = index;
            QuranTableView.ScrollIntoView(QuranTableView.SelectedItem);
        });
    }
    public async void SetAudioPlayerHostVisibility(Visibility visibility)
    {
        switch (visibility)
        {
            case Visibility.Visible:
                AnimateShowPanel(audioPlayerHostVisual, AudioPlayerHost);
                break;
            case Visibility.Collapsed:
                AnimateHidePanel(audioPlayerHostVisual, AudioPlayerHost);
                break;
        }
    }

    public async void SetAudioSource(string filePath)
    {
        if (File.Exists(filePath))
        {
            AudioPlayer.Source = MediaSource.CreateFromUri(new Uri(filePath));
            AudioPlayer.MediaPlayer.MediaEnded -= MediaPlayer_MediaEnded;
            AudioPlayer.MediaPlayer.MediaEnded += MediaPlayer_MediaEnded;

            if (QuranViewModel.IsAutoPlayNextFile && isAnyFilePlayed)
            {
                AudioPlayer.MediaPlayer.Play();
            }
        }
        else
        {
            if (Settings.IsAutoDownloadAudio)
            {
                try
                {
                    download = DownloadBuilder.New()
                       .WithUrl(Path.Combine(Settings.Audio.Url, Path.GetFileName(filePath)))
                       .WithDirectory(Path.Combine(Settings.AudiosPath, Settings.Audio.DirName))
                       .Build();

                    download.DownloadProgressChanged -= DownloadProgressChanged;
                    download.DownloadProgressChanged += DownloadProgressChanged;
                    download.DownloadFileCompleted -= DownloadFileCompleted;
                    download.DownloadFileCompleted += DownloadFileCompleted;
                    download.DownloadStarted -= DownloadStarted;
                    download.DownloadStarted += DownloadStarted;

                    await download.StartAsync();
                }
                catch (Exception ex)
                {
                    Logger?.Error(ex, ex.Message);
                    await MessageBox.ShowErrorAsync(ex.Message, Strings.MessageBoxErrorTitle.GetLocalizedResource());
                }
            }
        }
    }
    private void DownloadStarted(object sender, DownloadStartedEventArgs e)
    {
        dispatcherQueue.TryEnqueue(() =>
        {
            AudioStorageBar.Visibility = Visibility.Visible;
        });
    }

    private void DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
    {
        dispatcherQueue.TryEnqueue(() =>
        {
            if (e.UserState is DownloadPackage package)
            {
                AudioStorageBar.Visibility = Visibility.Collapsed;

                SetAudioSource(package.FileName);
            }
        });
    }

    private void DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
    {
        dispatcherQueue.TryEnqueue(() =>
        {
            AudioStorageBar.Value = e.ProgressPercentage;
        });
    }
    private void MediaPlayer_MediaEnded(Windows.Media.Playback.MediaPlayer sender, object args)
    {
        DispatcherQueue.TryEnqueue(() =>
        {
            isAnyFilePlayed = true;
            if (QuranViewModel.IsAutoPlayNextFile)
            {
                GoToNextItem();
            }
        });
    }

    public void StopAudio()
    {
        var mediaPlayer = AudioPlayer.MediaPlayer;
        if (mediaPlayer != null)
        {
            mediaPlayer.Pause();
            mediaPlayer.PlaybackSession.Position = TimeSpan.Zero;
        }
    }

    public void DisposeAudio()
    {
        DispatcherQueue.TryEnqueue(() =>
        {
            AudioPlayer.MediaPlayer.Dispose();
        });
    }

    private async void OnToggleButtonChecked(object sender, RoutedEventArgs e)
    {
        try
        {
            var tg = sender as ToggleButton;
            if (tg != null && tg.DataContext is FinalQuran finalQuran)
            {
                using var db = new AlAnvarDBContext();
                if (tg.IsChecked.Value)
                {
                    var fav = new QuranFavoriteTable
                    {
                        AyaId = finalQuran.AyaId,
                        SuraId = finalQuran.SuraId,
                    };

                    await db.Favorites.AddAsync(fav);
                    await db.SaveChangesAsync();
                    tg.Content = new FontIcon() { Glyph = "\uE735", FontSize = 16 };
                }
                else
                {
                    var result = await Queries.GetFavoriteByIdsQueryAsync(db, finalQuran.SuraId, finalQuran.AyaId).FirstOrDefaultAsync();
                    if (result != null)
                    {
                        db.Favorites.Remove(result);
                        await db.SaveChangesAsync();
                    }
                    tg.Content = new FontIcon() { Glyph = "\uE734", FontSize = 16 };
                }
            }
        }
        catch (Exception ex)
        {
            Logger?.Error(ex, ex.Message);
            await MessageBox.ShowErrorAsync(ex.Message, Strings.MessageBoxErrorTitle.GetLocalizedResource());
        }
    }

    private async void BtnNote_Click(object sender, RoutedEventArgs e)
    {
        if (!await EnsureDatabaseExistsAsync())
            return;

        var button = sender as Button;
        if (button != null && button.DataContext is FinalQuran finalQuran)
        {
            var dialog = Resources["AddNoteDialog"] as WindowedContentDialog;
            if (dialog != null)
            {
                ViewModel.SuraNote = Metadata.Name;
                ViewModel.VerseNote = finalQuran.Aya;
                ViewModel.CanSaveNote = false;
                ViewModel.TitleNote = string.Empty;
                ViewModel.DescriptionNote = string.Empty;

                noteSuraId = finalQuran.SuraId;
                noteVerseId = finalQuran.AyaId;

                dialog.OwnerWindow = App.MainWindow;

                dialog.PrimaryButtonClick -= OnDialogPrimaryButtonClick;
                dialog.PrimaryButtonClick += OnDialogPrimaryButtonClick;

                await dialog.ShowAsync(true);
            }
        }
    }

    private async void OnDialogPrimaryButtonClick(WindowedContentDialog sender, System.ComponentModel.CancelEventArgs args)
    {
        try
        {
            using var db = new AlAnvarDBContext();
            var note = new QuranNoteTable
            {
                SuraId = noteSuraId,
                AyaId = noteVerseId,
                Title = ViewModel.TitleNote,
                Description = ViewModel.DescriptionNote,
                CreatedAt = DateTime.Now.ToShortDateString(),
                UpdatedAt = DateTime.Now.ToShortDateString(),
            };
            await db.Notes.AddAsync(note);
            await db.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            Logger?.Error(ex, ex.Message);
            await MessageBox.ShowErrorAsync(ex.Message, Strings.MessageBoxErrorTitle.GetLocalizedResource());
        }
    }

    private void BtnCopy_Click(object sender, RoutedEventArgs e)
    {
        var button = sender as Button;
        if (button != null && button.DataContext is FinalQuran finalQuran)
        {            
            var strBuilder = new StringBuilder();
            strBuilder.AppendLine(finalQuran.Aya);
            if (!string.IsNullOrEmpty(finalQuran.Translation))
            {
                strBuilder.AppendLine(finalQuran.Translation);
            }

            CopyToClipboard(strBuilder.ToString());
        }
    }

    public async void Refresh()
    {
        await ViewModel.OnPageLoaded(Metadata);
    }

    public async void OnMenuClicked(object sender, RoutedEventArgs e)
    {
        var menu = sender as MenuFlyoutItem;
        if (menu != null && menu.Tag != null && menu.DataContext is FinalQuran finalQuran)
        {
            string suraStr = finalQuran.SuraId.ToString("D3"); // Format SuraId as XXX

            switch (menu.Tag.ToString())
            {
                case "CopyVerse":
                    CopyToClipboard(finalQuran.Aya);
                    break;
                case "CopyTranslation":
                    CopyToClipboard(finalQuran.Translation);
                    break;
                case "CopyAll":
                    StringBuilder stringBuilder = new StringBuilder();
                    stringBuilder.AppendLine(finalQuran.Aya);
                    stringBuilder.AppendLine(finalQuran.Translation);

                    CopyToClipboard(stringBuilder.ToString());
                    break;
                case "Favorite":
                    if (menu.CommandParameter is ToggleButton toggleButton)
                    {
                        toggleButton.IsChecked = !toggleButton.IsChecked;
                    }
                    break;
                case "Tafsir":
                    var tafsirType = GeneralHelper.GetEnum<TafsirType>(menu.CommandParameter?.ToString());
                    QuranPage.Instance.AddNewTafsirTab(finalQuran, menu.Text, tafsirType);
                    break;
                case "Play":
                    var selectedItem = QuranTableView.Items.OfType<FinalQuran>().Where(x => x.SuraId == finalQuran.SuraId).FirstOrDefault();
                    var index = QuranTableView.Items.IndexOf(selectedItem);
                    SetSelectedIndex(index);
                    AudioPlayer.MediaPlayer.Play();
                    break;
                case "Export":
                    if (Settings.Audio == null)
                    {
                        await MessageBox.ShowErrorAsync(Strings.QuranTabViewItem_MessageBoxExportError.GetLocalizedResource(), Strings.MessageBoxErrorTitle.GetLocalizedResource());
                        return;
                    }
                    var picker = new FileSavePicker(App.MainWindow.AppWindow.Id);
                    picker.FileTypeChoices.Add("Audio File", new List<string>() { ".mp3" });
                    picker.SuggestedFileName = $"{finalQuran.SuraFinglishName}-{Settings.Audio.DirName}-{finalQuran.AyaId.ToString("D3")}";
                    var result = await picker.PickSaveFileAsync();
                    if (result != null)
                    {
                        var filePath = Path.Combine(Settings.AudiosPath, Settings.Audio.DirName, $"{finalQuran.AudioFileName}.mp3");
                        if (File.Exists(filePath))
                        {
                            File.Copy(filePath, result.Path, true);
                        }
                        else
                        {
                            await MessageBox.ShowErrorAsync(Strings.QuranTabViewItem_MessageBoxExportFileError.GetLocalizedResource(), Strings.MessageBoxErrorTitle.GetLocalizedResource());
                        }
                    }
                    break;
                case "ExportAll":
                    if (Settings.Audio == null)
                    {
                        await MessageBox.ShowErrorAsync(Strings.QuranTabViewItem_MessageBoxExportError.GetLocalizedResource(), Strings.MessageBoxErrorTitle.GetLocalizedResource());
                        return;
                    }
                    var folderPicker = new FolderPicker(App.MainWindow.AppWindow.Id);
                    var folderResult = await folderPicker.PickSingleFolderAsync();
                    if (folderResult != null)
                    {
                        var folderPath = Path.Combine(Settings.AudiosPath, Settings.Audio.DirName);
                        if (Directory.Exists(folderPath))
                        {
                            var files = Directory.EnumerateFiles(folderPath, "*.mp3", SearchOption.AllDirectories).Where(path =>
                            {
                                string fileName = Path.GetFileNameWithoutExtension(path);

                                // Make sure filename is at least 3 digits
                                if (fileName.Length < 3)
                                    return false;

                                string suraPart = fileName.Substring(0, 3);
                                return suraPart == suraStr;
                            });

                            foreach (var item in files)
                            {
                                var fileName = Path.GetFileNameWithoutExtension(item);
                                fileName = fileName.Substring(3, 3);
                                File.Copy(item, Path.Combine(folderResult.Path, $"{finalQuran.SuraFinglishName}-{Settings.Audio.DirName}-{fileName}.mp3"), true);
                            }
                        }
                        else
                        {
                            await MessageBox.ShowErrorAsync(Strings.QuranTabViewItem_MessageBoxExportFileError.GetLocalizedResource(), Strings.MessageBoxErrorTitle.GetLocalizedResource());
                        }
                    }
                    break;
                case "ExportCustom":
                    if (Settings.Audio == null)
                    {
                        await MessageBox.ShowErrorAsync(Strings.QuranTabViewItem_MessageBoxExportError.GetLocalizedResource(), Strings.MessageBoxErrorTitle.GetLocalizedResource());
                        return;
                    }

                    var dialog = new WindowedContentDialog()
                    {
                        Title = Strings.QuranTabViewItem_DialogTitle.GetLocalizedResource(),
                        PrimaryButtonText = Strings.QuranTabViewItem_DialogPrimaryButtonText.GetLocalizedResource(),
                        SecondaryButtonText = Strings.QuranTabViewItem_DialogSecondaryButtonText.GetLocalizedResource(),
                        DefaultButton = ContentDialogButton.Primary,
                        OwnerWindow = App.MainWindow,
                        HasTitleBar = false,
                        ContentMinWidth = 500,
                        ContentFlowDirection = GeneralHelper.GetEnum<FlowDirection>(Strings.Main_FlowDirection_FlowDirection.GetLocalizedResource())
                    };

                    var fromNumberBox = new NumberBox
                    {
                        SpinButtonPlacementMode = NumberBoxSpinButtonPlacementMode.Inline,
                        Minimum = 1,
                        Maximum = Metadata.Aya,
                        Value = 1,
                        Header = Strings.QuranTabViewItem_DialogExportFromHeader.GetLocalizedResource()
                    };
                    var toNumberBox = new NumberBox
                    {
                        SpinButtonPlacementMode = NumberBoxSpinButtonPlacementMode.Inline,
                        Minimum = fromNumberBox.Value,
                        Maximum = Metadata.Aya,
                        Value = Metadata.Aya,
                        Header = Strings.QuranTabViewItem_DialogExportToHeader.GetLocalizedResource()
                    };

                    var stckPanel = new StackPanel
                    {
                        Spacing = 10
                    };

                    stckPanel.Children.Add(fromNumberBox);
                    stckPanel.Children.Add(toNumberBox);

                    dialog.Content = stckPanel;

                    dialog.PrimaryButtonClick -= OnPrimaryClick;
                    dialog.PrimaryButtonClick += OnPrimaryClick;

                    await dialog.ShowAsync(true);

                    async void OnPrimaryClick(WindowedContentDialog sender, System.ComponentModel.CancelEventArgs args)
                    {
                        var folderPath = Path.Combine(Settings.AudiosPath, Settings.Audio.DirName);
                        if (Directory.Exists(folderPath))
                        {
                            var files = Directory.EnumerateFiles(folderPath, "*.mp3", SearchOption.AllDirectories).Where(path =>
                            {
                                string name = Path.GetFileNameWithoutExtension(path);

                                // Must be exactly 6 digits like 001002
                                if (name.Length != 6 || !name.All(char.IsDigit))
                                    return false;

                                string suraPart = name.Substring(0, 3);
                                string ayaPart = name.Substring(3, 3);

                                if (suraPart != suraStr)
                                    return false;

                                int aya = int.Parse(ayaPart);

                                return aya >= fromNumberBox.Value && aya <= toNumberBox.Value;
                            });

                            var folderPicker = new FolderPicker(App.MainWindow.AppWindow.Id);
                            var folderResult = await folderPicker.PickSingleFolderAsync();
                            if (folderResult != null)
                            {
                                foreach (var item in files)
                                {
                                    var fileName = Path.GetFileNameWithoutExtension(item);
                                    fileName = fileName.Substring(3, 3);
                                    File.Copy(item, Path.Combine(folderResult.Path, $"{finalQuran.SuraFinglishName}-{Settings.Audio.DirName}-{fileName}.mp3"), true);
                                }
                            }
                        }
                        else
                        {
                            await MessageBox.ShowErrorAsync(Strings.QuranTabViewItem_MessageBoxExportFileError.GetLocalizedResource(), Strings.MessageBoxErrorTitle.GetLocalizedResource());
                        }
                    }
                    break;
            }
        }
    }

    public TableView GetTableView()
    {
        return QuranTableView;
    }
}
