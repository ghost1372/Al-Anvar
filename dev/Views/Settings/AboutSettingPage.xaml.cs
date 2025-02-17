using Microsoft.UI.Xaml.Navigation;

namespace AlAnvar.Views;

public sealed partial class AboutSettingPage : Page
{
    public AboutViewModel ViewModel { get; }
    public AboutSettingPage()
    {
        ViewModel = App.GetService<AboutViewModel>();
        this.InitializeComponent();
    }
}
