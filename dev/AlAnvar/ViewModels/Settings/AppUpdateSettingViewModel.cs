using Microsoft.UI.Xaml.Media;
using Windows.System;

namespace AlAnvar.ViewModels;

public partial class AppUpdateSettingViewModel : ObservableObject
{
    [ObservableProperty]
    public partial string CurrentVersion { get; set; }

    [ObservableProperty]
    public partial string LastUpdateCheck { get; set; }

    [ObservableProperty]
    public partial bool IsUpdateAvailable { get; set; }

    [ObservableProperty]
    public partial bool IsLoading { get; set; }

    [ObservableProperty]
    public partial bool IsCheckButtonEnabled { get; set; } = true;

    [ObservableProperty]
    public partial string LoadingStatus { get; set; }

    private string ChangeLog = string.Empty;

    public AppUpdateSettingViewModel()
    {
        CurrentVersion = $"{Strings.AppUpdateSettingViewModel_CurrentVersion.GetLocalizedResource()} {ProcessInfoHelper.VersionWithPrefix}";
        LastUpdateCheck = Settings.LastUpdateCheck;
    }

    [RelayCommand]
    private async Task CheckForUpdateAsync()
    {
        IsLoading = true;
        IsUpdateAvailable = false;
        IsCheckButtonEnabled = false;
        LoadingStatus = Strings.AppUpdateSettingViewModel_CheckVersion.GetLocalizedResource();
        if (NetworkHelper.IsNetworkAvailable())
        {
            try
            {
                string username = Constants.Username;
                string repo = Constants.RepoName;
                LastUpdateCheck = DateTime.Now.ToShortDateString();
                Settings.LastUpdateCheck = DateTime.Now.ToShortDateString();
                var update = await UpdateHelper.CheckUpdateAsync(username, repo, new Version(ProcessInfoHelper.Version));
                if (update.StableRelease.IsExistNewVersion)
                {
                    IsUpdateAvailable = true;
                    ChangeLog = update.StableRelease.Changelog;
                    LoadingStatus = string.Format(Strings.AppUpdateSettingViewModel_FoundVersion.GetLocalizedResource(), update.StableRelease.TagName, update.StableRelease.CreatedAt, update.StableRelease.PublishedAt);
                }
                else if (update.PreRelease.IsExistNewVersion)
                {
                    IsUpdateAvailable = true;
                    ChangeLog = update.PreRelease.Changelog;
                    LoadingStatus = string.Format(Strings.AppUpdateSettingViewModel_FoundPreReleaseVersion.GetLocalizedResource(), update.PreRelease.TagName, update.PreRelease.CreatedAt, update.PreRelease.PublishedAt);
                }
                else
                {
                    LoadingStatus = Strings.AppUpdateSettingViewModel_NoNewVersion.GetLocalizedResource();
                }
            }
            catch (Exception ex)
            {
                LoadingStatus = ex.Message;
                IsLoading = false;
                IsCheckButtonEnabled = true;
                Logger?.Error(ex, ex.Message);
            }
        }
        else
        {
            LoadingStatus = Strings.AppUpdateSettingViewModel_ErrorConnection.GetLocalizedResource();
        }
        IsLoading = false;
        IsCheckButtonEnabled = true;
    }

    [RelayCommand]
    private async Task GoToUpdateAsync()
    {
        await Launcher.LaunchUriAsync(new Uri($"https://github.com/{Constants.Username}/{Constants.RepoName}/releases"));
    }

    [RelayCommand]
    private async Task GetReleaseNotesAsync()
    {
        WindowedContentDialog dialog = new WindowedContentDialog()
        {
            Title = Strings.AppUpdateSettingViewModel_ReleaseNoteTitle.GetLocalizedResource(),
            CloseButtonText = Strings.AppUpdateSettingViewModel_ReleaseNoteClose.GetLocalizedResource(),
            ContentFlowDirection = GeneralHelper.GetEnum<FlowDirection>(Strings.Main_FlowDirection_FlowDirection.GetLocalizedResource()),
            Content = new ScrollViewer
            {
                Content = new TextBlock
                {
                    Text = ChangeLog,
                    Margin = new Thickness(10)
                },
                Margin = new Thickness(10)
            },
            Margin = new Thickness(10),
            DefaultButton = ContentDialogButton.Close,
            OwnerWindow = App.MainWindow,
            Underlay = UnderlayMode.SmokeLayer,
            SystemBackdrop = new MicaBackdrop()
        };

        await dialog.ShowAsync(true);
    }
}
