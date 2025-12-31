using Windows.System;

namespace AlAnvar.Views;

public sealed partial class HomeLandingPage : Page
{
    public HomeLandingViewModel ViewModel { get; }
    public HomeLandingPage()
    {
        ViewModel = App.GetService<HomeLandingViewModel>();
        this.InitializeComponent();
        Loaded -= HomeLandingPage_Loaded;
        Loaded += HomeLandingPage_Loaded;
    }

    private async void HomeLandingPage_Loaded(object sender, RoutedEventArgs e)
    {
        await EnsureDatabaseExistsAsync();

        ViewModel.CheckForUpdateAsync();
    }

    private async void OnCheckUpdateControl(object sender, RoutedEventArgs e)
    {
        await Launcher.LaunchUriAsync(new Uri(Constants.RepoReleaseUrl));
    }
}

