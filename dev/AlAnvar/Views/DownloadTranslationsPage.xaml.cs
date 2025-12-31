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

    private WindowedContentDialog dialog;
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
                Title = Strings.DownloadTranslationsPage_DialogTitle.GetLocalizedResource(),
                PrimaryButtonText = Strings.DownloadTranslationsPage_DialogPrimaryButtonText.GetLocalizedResource(),
                SecondaryButtonText = Strings.DownloadTranslationsPage_DialogSecondaryButtonText.GetLocalizedResource(),
                DefaultButton = ContentDialogButton.Primary,
                OwnerWindow = App.MainWindow,
                HasTitleBar = false,
                ContentMinWidth = 500,
                ContentFlowDirection = GeneralHelper.GetEnum<FlowDirection>(Strings.Main_FlowDirection_FlowDirection.GetLocalizedResource())
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

            await dialog.ShowAsync(true);
        }
    }

    private void OnDialogSecondaryButtonClick(WindowedContentDialog sender, CancelEventArgs args)
    {
        dispatcherQueue.TryEnqueue(() =>
        {
            download?.Stop();
            dialog?.PrimaryButtonText = Strings.DownloadTranslationsPage_DialogCancelText.GetLocalizedResource();
            storageBarTranslation?.PercentCaution = 10;
        });
    }

    private async void OnDialogPrimaryButtonClick(WindowedContentDialog sender, CancelEventArgs args)
    {
        args.Cancel = true;

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
            dialog?.PrimaryButtonText = Strings.DownloadTranslationsPage_FileDownloaded.GetLocalizedResource();
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
