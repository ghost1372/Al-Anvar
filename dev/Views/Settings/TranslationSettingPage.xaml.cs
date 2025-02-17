using Microsoft.UI.Xaml.Navigation;

namespace AlAnvar.Views;

public sealed partial class TranslationSettingPage : Page
{
    public TranslationSettingViewModel ViewModel { get; }
    public TranslationSettingPage()
    {
        ViewModel = App.GetService<TranslationSettingViewModel>();
        this.InitializeComponent();
    }
}
