namespace AlAnvar.Views;

public sealed partial class AudioSettingPage : Page
{
    public AudioSettingViewModel ViewModel { get; }
    public AudioSettingPage()
    {
        ViewModel = App.GetService<AudioSettingViewModel>();
        InitializeComponent();

        Loaded -= OnLoaded;
        Loaded += OnLoaded;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (await EnsureDatabaseExistsAsync())
        {
            await ViewModel.GetAvailableAudios();
        }
    }
}
