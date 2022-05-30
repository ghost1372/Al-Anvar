namespace AlAnvar.UI.TabViewItems;

public sealed partial class SingleTafsirTabViewItem : TabViewItem
{
    public static readonly DependencyProperty QuranItemProperty =
        DependencyProperty.Register("QuranItem", typeof(QuranItem), typeof(QuranTabViewItem),
        new PropertyMetadata(null));

    public QuranItem QuranItem
    {
        get { return (QuranItem) GetValue(QuranItemProperty); }
        set { SetValue(QuranItemProperty, value); }
    }

    public static readonly DependencyProperty ChapterProperty =
        DependencyProperty.Register("Chapter", typeof(ChapterProperty), typeof(QuranTabViewItem),
        new PropertyMetadata(null));

    public ChapterProperty Chapter
    {
        get { return (ChapterProperty) GetValue(ChapterProperty); }
        set { SetValue(ChapterProperty, value); }
    }

    public SingleTafsirTabViewItem()
    {
        this.InitializeComponent();
        Loaded += SingleTafsirTabViewItem_Loaded;
    }

    private async void SingleTafsirTabViewItem_Loaded(object sender, RoutedEventArgs e)
    {
        await Task.Run(() =>
        {
            LoadTafsirsInCombobox();
        });
        SetAyaOrTranslationText();
        prgLoading.IsActive = false;
    }
    private void RadioButtons_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        SetAyaOrTranslationText();
    }
    public void SetAyaOrTranslationText()
    {
        if (radioButtons.SelectedIndex == 0)
        {
            txtAya.Text = QuranItem.AyahText;
        }
        else
        {
            txtAya.Text = QuranItem.TranslationText;
        }
        txtAya.Header = $"{QuranItem.SurahName} {QuranItem.AyahNumber}:{Chapter.Ayas}";
    }

    private void cmbTafsir_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        Settings.QuranTafsir = cmbTafsir.SelectedItem as TafsirName;
        SetTafsirText();
    }

    public async void SetTafsirText()
    {
        var selectedTafsir = Settings.QuranTafsir ?? GetComboboxFirstElement<TafsirName>(cmbTafsir);
        if (selectedTafsir is not null)
        {
            using var db = new AlAnvarDBContext();
            var tafsir = await db.Tafsirs.Where(x => x.IdName == selectedTafsir.Id && x.IdVerse.Contains($"[{QuranItem.Id}]")).FirstOrDefaultAsync();
            txtTafsir.Document.SetText(Microsoft.UI.Text.TextSetOptions.None, tafsir.Value);
        }
    }
    private void LoadTafsirsInCombobox()
    {
        DispatcherQueue.TryEnqueue(async () =>
        {
            using var db = new AlAnvarDBContext();
            cmbTafsir.ItemsSource = await db.TafsirNames.ToListAsync();
            cmbTafsir.SelectedItem = cmbTafsir.Items.Where(trans => ((TafsirName) trans).Name == Settings.QuranTafsir?.Name)?.FirstOrDefault();
        });
    }
}
