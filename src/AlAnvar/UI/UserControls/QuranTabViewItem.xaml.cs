namespace AlAnvar.UI.UserControls;

public sealed partial class QuranTabViewItem : TabViewItem
{
    #region DependencyProperty
    public static readonly DependencyProperty SurahIdProperty =
        DependencyProperty.Register("SurahId", typeof(int), typeof(QuranTabViewItem),
        new PropertyMetadata(1));

    public int SurahId
    {
        get { return (int)GetValue(SurahIdProperty); }
        set { SetValue(SurahIdProperty, value); }
    }

    public static readonly DependencyProperty TotalAyahProperty =
        DependencyProperty.Register("TotalAyah", typeof(int), typeof(QuranTabViewItem),
        new PropertyMetadata(0));

    public int TotalAyah
    {
        get { return (int)GetValue(TotalAyahProperty); }
        set { SetValue(TotalAyahProperty, value); }
    }
    #endregion

    public ObservableCollection<QuranItem> QuranCollection { get; set; } = new ObservableCollection<QuranItem>();
    private List<TranslationItem> TranslationCollection { get; set; } = new List<TranslationItem>();
    internal static QuranTabViewItem Instance { get; private set; }

    public QuranTabViewItem()
    {
        this.InitializeComponent();
        Instance = this;
        DataContext = this;
        Loaded += QuranTabViewItem_LoadedAsync;
    }

    private void QuranTabViewItem_LoadedAsync(object sender, RoutedEventArgs e)
    {
        GetQuranText();
    }
    public int GetCurrentListViewItemIndex()
    {
        return quranListView.SelectedIndex;
    }
    public void ScrollIntoView(int index)
    {
        quranListView.SelectedIndex = index;
        quranListView.ScrollIntoView(quranListView.SelectedItem);
    }
    public async void GetQuranText()
    {
        QuranCollection?.Clear();
        using var db = new AlAnvarDBContext();
        var surah = await db.Qurans.Where(x => x.SurahId == SurahId).ToListAsync();
        GetTranslation();

        foreach (var item in surah)
        {
            QuranCollection.Add(new QuranItem
            {
                Audio = item.Audio,
                AyahNumber = item.AyahNumber,
                AyahText = item.AyahText,
                Hizb = item.Hizb,
                Id = item.Id,
                Juz = item.Juz,
                SurahId = SurahId,
                TotalAyah = TotalAyah,
                AyaDetail = $"({item.AyahNumber}:{TotalAyah})",
                TranslationText = TranslationCollection.Where(x => x.Aya == item.AyahNumber).FirstOrDefault()?.Translation
            });
        }
        quranListView.ItemsSource = QuranCollection;
    }

    public void GetTranslation()
    {
        TranslationCollection?.Clear();
        var selectedTranslation = Settings.QuranTranslation;
        if (Directory.Exists(Constants.TranslationsPath))
        {
            var files = Directory.GetFiles(Constants.TranslationsPath, "*.txt", SearchOption.AllDirectories);
            if (files.Count() > 0)
            {
                foreach (var item in files)
                {
                    if (Path.GetFileNameWithoutExtension(item).Equals(selectedTranslation.TranslationId))
                    {
                        var lines = File.ReadAllLines(item);
                        foreach (var line in lines)
                        {
                            var trans = line.Split("|");
                            if (trans[0] == SurahId.ToString())
                            {
                                TranslationCollection.Add(new TranslationItem
                                {
                                    SurahId = Convert.ToInt32(trans[0]),
                                    Aya = Convert.ToInt32(trans[1]),
                                    Translation = trans[2]
                                });
                            }
                        }
                    }
                }
            }
        }
    }
}
