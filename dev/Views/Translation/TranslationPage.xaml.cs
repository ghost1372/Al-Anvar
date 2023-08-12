namespace AlAnvar.Views;

public sealed partial class TranslationPage : Page
{
    public static TranslationPage Instance { get; set; }
    public TranslationPage()
    {
        this.InitializeComponent();
        Instance = this;
        Loaded += TranslationPage_Loaded;
    }

    private void TranslationPage_Loaded(object sender, RoutedEventArgs e)
    {
        segmented.SelectedIndex = 0;
    }

    public Frame GetFrame()
    {
        return translationFrame;
    }

    private void OnSegmentedChanged(object sender, SelectionChangedEventArgs e)
    {
        var segmented = sender as Segmented;
        if (translationFrame != null)
        {
            if (segmented.SelectedIndex == 0)
            {
                translationFrame.Navigate(typeof(DownloadTranslationPage));
            }
            else
            {
                translationFrame.Navigate(typeof(OfflineTranslationPage));
            }
        }
    }
}
