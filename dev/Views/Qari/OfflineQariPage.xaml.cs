namespace AlAnvar.Views;

public sealed partial class OfflineQariPage : Page
{
    public OfflineQariViewModel ViewModel { get; }

    public OfflineQariPage()
    {
        ViewModel = App.GetService<OfflineQariViewModel>();
        this.InitializeComponent();
    }
}
