namespace AlAnvar.Views;

public sealed partial class TranslationSettingPage : Page
{
    public TranslationSettingViewModel ViewModel { get; }
    public TranslationSettingPage()
    {
        ViewModel = App.GetService<TranslationSettingViewModel>();
        InitializeComponent();
        Loaded -= OnLoaded;
        Loaded += OnLoaded;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (await EnsureDatabaseExistsAsync())
        {
            await ViewModel.GetAvailableTranslations();
        }
    }
}
