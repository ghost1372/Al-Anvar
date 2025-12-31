using AlAnvar.Models;

namespace AlAnvar.Views;

public sealed partial class GeneralSettingPage : Page
{
    public GeneralSettingViewModel ViewModel { get; }

    public GeneralSettingPage()
    {
        ViewModel = App.GetService<GeneralSettingViewModel>();
        this.InitializeComponent();
        Loaded += GeneralSettingPage_Loaded;
    }

    private void GeneralSettingPage_Loaded(object sender, RoutedEventArgs e)
    {
        var uiFont = CmbUIFont.Items.OfType<FontOption>().Where(x => x.FontKey == Settings.UIFont.FontKey).FirstOrDefault();
        var quranFont = CmbQuranFont.Items.OfType<FontOption>().Where(x => x.FontKey == Settings.QuranFont.FontKey).FirstOrDefault();
        var translationFont = CmbTranslationFont.Items.OfType<FontOption>().Where(x => x.FontKey == Settings.TranslationFont.FontKey).FirstOrDefault();
        CmbUIFont.SelectedItem = uiFont;
        CmbQuranFont.SelectedItem = quranFont;
        CmbTranslationFont.SelectedItem = translationFont;
    }

    private async void NavigateToLogPath_Click(object sender, RoutedEventArgs e)
    {
        string folderPath = (sender as HyperlinkButton).Content.ToString();
        if (Directory.Exists(folderPath))
        {
            Windows.Storage.StorageFolder folder = await Windows.Storage.StorageFolder.GetFolderFromPathAsync(folderPath);
            await Windows.System.Launcher.LaunchFolderAsync(folder);
        }
    }

    private async void OnUIFontSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (CmbUIFont.SelectedItem is FontOption font)
        {
            if (font.FontKey == Settings.UIFont.FontKey)
                return;

            Settings.UIFont = font;
            FontHelper.SetUIFontFamily(font);

            ViewModel.ShowRestartForFont = true;
        }
    }
    private async void OnQuranFontSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (CmbQuranFont.SelectedItem is FontOption font)
        {
            if (font.FontKey == Settings.QuranFont.FontKey)
                return;

            Settings.QuranFont = font;
            FontHelper.SetQuranFontFamily(font);
        }
    }
    private async void OnTranslationFontSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (CmbTranslationFont.SelectedItem is FontOption font)
        {
            if (font.FontKey == Settings.TranslationFont.FontKey)
                return;

            Settings.TranslationFont = font;
            FontHelper.SetTranslationFontFamily(font);
        }
    }
}


