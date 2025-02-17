using Microsoft.UI.Xaml.Navigation;

namespace AlAnvar.Views;

public sealed partial class QariSettingPage : Page
{
    public QariSettingViewModel ViewModel { get; }
    public QariSettingPage()
    {
        ViewModel = App.GetService<QariSettingViewModel>();
        this.InitializeComponent();
    }
}
