using Microsoft.UI.Xaml.Navigation;

namespace AlAnvar.Views;

public sealed partial class FontSettingPage : Page
{
    public FontSettingViewModel ViewModel { get; }
    public string BreadCrumbBarItemText { get; set; }

    public FontSettingPage()
    {
        ViewModel = App.GetService<FontSettingViewModel>();
        this.InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        BreadCrumbBarItemText = e.Parameter as string;
    }
}
