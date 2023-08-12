using Microsoft.UI.Xaml.Navigation;

namespace AlAnvar.Views;

public sealed partial class AboutSettingPage : Page
{
    public AboutViewModel ViewModel { get; }
    public string BreadCrumbBarItemText { get; set; }
    public AboutSettingPage()
    {
        ViewModel = App.GetService<AboutViewModel>();
        this.InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        BreadCrumbBarItemText = e.Parameter as string;
    }
}
