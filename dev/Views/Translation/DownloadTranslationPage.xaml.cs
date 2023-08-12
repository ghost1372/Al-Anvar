namespace AlAnvar.Views;

public sealed partial class DownloadTranslationPage : Page
{
    public DownloadTranslationViewModel ViewModel { get; }

    public static DownloadTranslationPage Instance { get; set; }
    public DownloadTranslationPage()
    {
        ViewModel = App.GetService<DownloadTranslationViewModel>();
        this.InitializeComponent();
        Instance = this;
    }
}
