using Microsoft.UI.Xaml.Navigation;

namespace AlAnvar.Views;

public sealed partial class QariSettingPage : Page
{
    public QariSettingViewModel ViewModel { get; }
    public string BreadCrumbBarItemText { get; set; }

    public QariSettingPage()
    {
        ViewModel = App.GetService<QariSettingViewModel>();
        this.InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        BreadCrumbBarItemText = e.Parameter as string;
    }
}
