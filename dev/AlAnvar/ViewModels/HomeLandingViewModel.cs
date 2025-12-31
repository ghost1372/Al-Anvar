namespace AlAnvar.ViewModels;

public partial class HomeLandingViewModel : ObservableObject
{
    [ObservableProperty]
    public partial string LastUpdateCheck { get; set; }

    [ObservableProperty]
    public partial bool IsUpdateAvailable { get; set; }

    [ObservableProperty]
    public partial string NewUpdateVersion { get; set; }

    public async void CheckForUpdateAsync()
    {
        IsUpdateAvailable = false;
        LastUpdateCheck = Settings.LastUpdateCheck ?? DateTime.Now.ToShortDateString();

        if (NetworkHelper.IsNetworkAvailable())
        {
            try
            {
                Settings.LastUpdateCheck = DateTime.Now.ToShortDateString();
                var update = await UpdateHelper.CheckUpdateAsync(Constants.Username, Constants.RepoName, new Version(ProcessInfoHelper.Version));
                if (update.StableRelease.IsExistNewVersion)
                {
                    IsUpdateAvailable = true;
                    NewUpdateVersion = update.StableRelease.TagName;
                }
                else if (update.PreRelease.IsExistNewVersion)
                {
                    IsUpdateAvailable = true;
                    NewUpdateVersion = update.PreRelease.TagName;
                }
                else
                {
                    IsUpdateAvailable = false;
                }
            }
            catch (Exception ex)
            {
                IsUpdateAvailable = false;
                Logger?.Error(ex, ex.Message);
            }
        }
    }
}
