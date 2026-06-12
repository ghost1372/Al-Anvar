using System.ComponentModel;
using AlAnvar.Models;
using Downloader;
using Microsoft.UI.Dispatching;
using WinUI.TableView;

namespace AlAnvar.Views;

public sealed partial class DownloadTranslationsPage : Page
{
    private readonly DispatcherQueue dispatcherQueue = DispatcherQueue.GetForCurrentThread();

    private IDownload download;

    private ContentDialogWindow dialog;
    private TranslationItem translation;
    private TextBlock txtStatus;
    private StorageBar storageBarTranslation;
    public static DownloadTranslationsPage Instance { get; private set; }
    public TranslationsViewModel ViewModel { get; }
    public DownloadTranslationsPage()
    {
        ViewModel = App.GetService<TranslationsViewModel>();
        InitializeComponent();
        Instance = this;

        TranslationsTableView.FilterDescriptions.Add(new FilterDescription(string.Empty, ViewModel.Filter));

        Loaded -= OnLoaded;
        Loaded += OnLoaded;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        await ViewModel.GetAvailableTranslations();
    }

    public TableView GetTableView()
    {
        return TranslationsTableView;
    }

    private async void TranslationsTableView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (TranslationsTableView.SelectedItem is TranslationItem translation)
        {
            this.translation = translation;

            var filePath = Path.Combine(Settings.TranslationsPath, translation.Id + ".json");
            if (File.Exists(filePath))
            {
                await MessageBox.ShowWarningAsync(Strings.DownloadTranslationsPage_FileAlreadyDownloaded.GetLocalizedResource(), Strings.DownloadTranslationsPage_FileAlreadyDownloadedTitle.GetLocalizedResource());
                return;
            }

            dialog = new()
            {
                Header = Strings.DownloadTranslationsPage_DialogTitle.GetLocalizedResource(),
                PrimaryButtonContent = Strings.DownloadTranslationsPage_DialogPrimaryButtonText.GetLocalizedResource(),
                SecondaryButtonContent = Strings.DownloadTranslationsPage_DialogSecondaryButtonText.GetLocalizedResource(),
                DefaultButton = ContentDialogButton.Primary,
                Owner = App.MainWindow,
                HasTitleBar = false,
                MinWidth = 500,
                SystemBackdrop = new Microsoft.UI.Xaml.Media.DesktopAcrylicBackdrop(),
                FlowDirection = GeneralHelper.GetEnum<FlowDirection>(Strings.Main_FlowDirection_FlowDirection.GetLocalizedResource())
            };

            StackPanel stck = new StackPanel
            {
                Spacing = 10,
                FlowDirection = FlowDirection.RightToLeft
            };
            txtStatus = new TextBlock
            {
                Text = translation.Name
            };

            storageBarTranslation = new StorageBar
            {
                Maximum = 100,
                Value = 0,
                FlowDirection = FlowDirection.LeftToRight,
                Percent = 0,
                PercentCaution = 1000,
                PercentCritical = 1001,
            };

            stck.Children.Add(txtStatus);
            stck.Children.Add(storageBarTranslation);
            dialog.Content = stck;

            dialog.PrimaryButtonClick -= OnDialogPrimaryButtonClick;
            dialog.PrimaryButtonClick += OnDialogPrimaryButtonClick;

            dialog.SecondaryButtonClick -= OnDialogSecondaryButtonClick;
            dialog.SecondaryButtonClick += OnDialogSecondaryButtonClick;

            await dialog.ShowDialogAsync();
        }
    }

    private void OnDialogSecondaryButtonClick(object sender, EventArgs args)
    {
        dispatcherQueue.TryEnqueue(() =>
        {
            download?.Stop();
            dialog?.PrimaryButtonContent = Strings.DownloadTranslationsPage_DialogCancelText.GetLocalizedResource();
            storageBarTranslation?.PercentCaution = 10;

            dialog.TryClose();
        });
    }

    private async void OnDialogPrimaryButtonClick(object sender, EventArgs args)
    {
        download = DownloadBuilder.New()
                        .WithUrl(translation?.Link)
                        .WithDirectory(Settings.TranslationsPath)
                        .Build();

        download.DownloadProgressChanged -= DownloadProgressChanged;
        download.DownloadProgressChanged += DownloadProgressChanged;
        download.DownloadFileCompleted -= DownloadFileCompleted;
        download.DownloadFileCompleted += DownloadFileCompleted;
        download.DownloadStarted -= DownloadStarted;
        download.DownloadStarted += DownloadStarted;

        await download.StartAsync();
    }

    private void DownloadStarted(object sender, DownloadStartedEventArgs e)
    {
        dispatcherQueue.TryEnqueue(() =>
        {
            dialog?.IsPrimaryButtonEnabled = false;
            storageBarTranslation?.PercentCaution = 1000;
        });
    }

    private void DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
    {
        dispatcherQueue.TryEnqueue(() =>
        {
            dialog?.PrimaryButtonContent = Strings.DownloadTranslationsPage_FileDownloaded.GetLocalizedResource();
        });
    }

    private void DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
    {
        dispatcherQueue.TryEnqueue(() =>
        {
            storageBarTranslation?.Value = e.ProgressPercentage;
        });
    }
}
