using Windows.System;

namespace AlAnvar.ViewModels;

public partial class AppUpdateSettingViewModel : ObservableObject
{
    [ObservableProperty]
    public string currentVersion;

    [ObservableProperty]
    public string lastUpdateCheck;

    [ObservableProperty]
    public bool isUpdateAvailable;

    [ObservableProperty]
    public bool isLoading;

    [ObservableProperty]
    public string loadingStatus = "وضعیت";

    private string changeLog = string.Empty;

    public AppUpdateSettingViewModel()
    {
        currentVersion = $"نسخه فعلی {ProcessInfoHelper.VersionWithPrefix}";
        lastUpdateCheck = Settings.LastUpdateCheck;
    }

    [RelayCommand]
    private async Task CheckForUpdateAsync()
    {
        IsLoading = true;
        IsUpdateAvailable = false;
        LoadingStatus = "درحال بررسی برای نسخه جدید";
        if (NetworkHelper.IsNetworkAvailable())
        {
            LastUpdateCheck = DateTime.Now.ToShortDateString();
            Settings.LastUpdateCheck = DateTime.Now.ToShortDateString();

            try
            {
                string username = "Ghost1372";
                string repo = "Al-Anvar";
                var update = await UpdateHelper.CheckUpdateAsync(username, repo, new Version(ProcessInfoHelper.Version));
                if (update.StableRelease.IsExistNewVersion)
                {
                    IsUpdateAvailable = true;
                    changeLog = update.StableRelease.Changelog;
                    LoadingStatus = $"ما یک نسخه جدید پیدا کردیم {update.StableRelease.TagName} در تاریخ {update.StableRelease.CreatedAt} ایجاد و در تاریخ {update.StableRelease.PublishedAt} منتشر شده است.";
                }
                else if (update.PreRelease.IsExistNewVersion)
                {
                    IsUpdateAvailable = true;
                    changeLog = update.PreRelease.Changelog;
                    LoadingStatus = $"ما یک نسخه پیشنمایش جدید پیدا کردیم {update.PreRelease.TagName} در تاریخ {update.PreRelease.CreatedAt} ایجاد و در تاریخ {update.PreRelease.PublishedAt} منتشر شده است.";
                }
                else
                {
                    LoadingStatus = "شما از آخرین نسخه استفاده می کنید";
                }
            }
            catch (Exception ex)
            {
                LoadingStatus = ex.Message;
            }
        }
        else
        {
            LoadingStatus = "خطا در اتصال";
        }
        IsLoading = false;
    }

    [RelayCommand]
    private async Task GetReleaseNotesAsync()
    {
        ContentDialog dialog = new ContentDialog()
        {
            Title = "یادداشت انتشار",
            CloseButtonText = "بستن",
            Content = new ScrollViewer
            {
                Content = new MarkdownTextBlock
                {
                    Text = changeLog,
                    CornerRadius = new CornerRadius(8),
                    Margin = new Thickness(10)
                },
                Margin = new Thickness(10)
            },
            Margin = new Thickness(10),
            DefaultButton = ContentDialogButton.Close,
            XamlRoot = App.MainWindow.Content.XamlRoot
        };

        await dialog.ShowAsync();
    }

    [RelayCommand]
    private async Task GoToUpdateAsync()
    {
        await Launcher.LaunchUriAsync(new Uri(ALAnvar_Repo));
    }
}
