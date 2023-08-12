using Microsoft.UI.Xaml.Navigation;

namespace AlAnvar.Views;

public sealed partial class TranslationSettingPage : Page
{
    public TranslationSettingViewModel ViewModel { get; }
    public string BreadCrumbBarItemText { get; set; }

    public TranslationSettingPage()
    {
        ViewModel = App.GetService<TranslationSettingViewModel>();
        this.InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        BreadCrumbBarItemText = e.Parameter as string;
    }
}
