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
        currentVersion = $"نسخه فعلی v{App.Current.AppVersion}";
        lastUpdateCheck = Settings.LastUpdateCheck;
    }

    [RelayCommand]
    private async Task CheckForUpdateAsync()
    {
        IsLoading = true;
        IsUpdateAvailable = false;
        LoadingStatus = "درحال بررسی برای نسخه جدید";
        if (ApplicationHelper.IsNetworkAvailable())
        {
            LastUpdateCheck = DateTime.Now.ToShortDateString();
            Settings.LastUpdateCheck = DateTime.Now.ToShortDateString();

            try
            {
                //TODO: Fix Project Name
                var update = await UpdateHelper.CheckUpdateAsync("ghost1372", "Al-Anvar", new Version(App.Current.AppVersion));
                if (update.IsExistNewVersion)
                {
                    IsUpdateAvailable = true;
                    changeLog = update.Changelog;
                    LoadingStatus = $"ما یک نسخه جدید پیدا کردیم {update.TagName} در تاریخ {update.CreatedAt} ایجاد و در تاریخ {update.PublishedAt} منتشر شده است.";
                }
                else
                {
                    LoadingStatus = "نسخه جدیدی پیدا نشد";
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
            XamlRoot = App.currentWindow.Content.XamlRoot
        };

        await dialog.ShowAsyncQueue();
    }

    [RelayCommand]
    private async Task GoToUpdateAsync()
    {
        await Launcher.LaunchUriAsync(new Uri(ALAnvar_Repo));
    }
}
