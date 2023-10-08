namespace AlAnvar.Views;

public sealed partial class QuranPage : Page
{
    public static QuranPage Instance { get; set; }
    public QuranViewModel ViewModel { get; }
    public QuranPage()
    {
        ViewModel = App.GetService<QuranViewModel>();
        this.InitializeComponent();
        Instance = this;
    }

    public QuranTabViewItem GetTabViewItem()
    {
        return tabView.SelectedItem as QuranTabViewItem;
    }

    private void tabView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var tabItem = tabView.SelectedItem as QuranTabViewItem;
        if (tabItem != null)
        {
            CmbTranslation.SelectedItem = tabItem.CurrentQuranTranslation;
            CmbTranslation.SelectedIndex = ViewModel.TranslationsCollection.IndexOf(tabItem.CurrentQuranTranslation);

            CmbQari.SelectedItem = tabItem.CurrentQuranAudio;
            CmbQari.SelectedIndex = ViewModel.QarisCollection.IndexOf(tabItem.CurrentQuranAudio);
            tabItem.SetAppBarToggleButtonValue();
        }
    }
}
