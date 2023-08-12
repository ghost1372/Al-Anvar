namespace AlAnvar.Views;

public sealed partial class OfflineTranslationPage : Page
{
    public OfflineTranslationViewModel ViewModel { get; }
    public OfflineTranslationPage()
    {
        ViewModel = App.GetService<OfflineTranslationViewModel>();
        this.InitializeComponent();
    }
}
