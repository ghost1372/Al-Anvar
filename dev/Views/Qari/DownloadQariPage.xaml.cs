namespace AlAnvar.Views;

public sealed partial class DownloadQariPage : Page
{
    public DownloadQariViewModel ViewModel { get; }

    public static DownloadQariPage Instance { get; set; }
    public DownloadQariPage()
    {
        ViewModel = App.GetService<DownloadQariViewModel>();
        this.InitializeComponent();
        Instance = this;
    }
}
