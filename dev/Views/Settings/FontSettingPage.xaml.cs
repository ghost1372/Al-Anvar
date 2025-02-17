using Microsoft.UI.Xaml.Navigation;

namespace AlAnvar.Views;

public sealed partial class FontSettingPage : Page
{
    public FontSettingViewModel ViewModel { get; }
    public static FontSettingPage Instance { get; set; }
    public FontSettingPage()
    {
        ViewModel = App.GetService<FontSettingViewModel>();
        this.InitializeComponent();
        Instance = this;
        Loaded += FontSettingPage_Loaded;
    }

    private void FontSettingPage_Loaded(object sender, RoutedEventArgs e)
    {
        RefreshTextBlockForeground();
    }

    public void RefreshTextBlockForeground()
    {
        if (Settings.AyatForeground != null)
        {
            TxtAyat.Foreground = ViewModel.TxtAyaForeground;
        }

        if (Settings.AyatNumberForeground != null)
        {
            TxtAyatNumber.Foreground = ViewModel.TxtAyaNumberForeground;
        }

        if (Settings.TranslationForeground != null)
        {
            TxtTranslation.Foreground = ViewModel.TxtTranslationForeground;
        }
    }

    public void RefreshTextBlockForeground2()
    {
        TxtAyat.Foreground = null;
        TxtAyatNumber.Foreground = null;
        TxtTranslation.Foreground = null;
    }
}
