using System.Runtime.CompilerServices;

namespace AlAnvar.UI.UserControls;

public sealed partial class QuranTabViewItem : TabViewItem, INotifyPropertyChanged
{
    #region DependencyProperty
    public static readonly DependencyProperty SurahNameProperty =
        DependencyProperty.Register("SurahName", typeof(int), typeof(QuranTabViewItem),
        new PropertyMetadata(1));

    public string SurahName
    {
        get { return (string) GetValue(SurahNameProperty); }
        set { SetValue(SurahNameProperty, value); }
    }

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

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    #region Property

    #region Translation
    private SolidColorBrush _TranslationForeground;

    public SolidColorBrush TranslationForeground
    {
        get { return _TranslationForeground; }
        set
        {
            _TranslationForeground = value;
            OnPropertyChanged();
        }
    }
    private FontFamily _TranslationFontFamily;

    public FontFamily TranslationFontFamily
    {
        get { return _TranslationFontFamily; }
        set
        {
            _TranslationFontFamily = value;
            OnPropertyChanged();
        }
    }
    private double _TranslationFontSize;

    public double TranslationFontSize
    {
        get { return _TranslationFontSize; }
        set
        {
            _TranslationFontSize = value;
            OnPropertyChanged();
        }
    }
    #endregion

    #region Ayat
    private SolidColorBrush _AyatForeground;

    public SolidColorBrush AyatForeground
    {
        get { return _AyatForeground; }
        set
        {
            _AyatForeground = value;
            OnPropertyChanged();
        }
    }
    private FontFamily _AyatFontFamily;

    public FontFamily AyatFontFamily
    {
        get { return _AyatFontFamily; }
        set
        {
            _AyatFontFamily = value;
            OnPropertyChanged();
        }
    }
    private double _AyatFontSize;

    public double AyatFontSize
    {
        get { return _AyatFontSize; }
        set
        {
            _AyatFontSize = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #region Ayat Number
    private SolidColorBrush _AyatNumberForeground;

    public SolidColorBrush AyatNumberForeground
    {
        get { return _AyatNumberForeground; }
        set
        {
            _AyatNumberForeground = value;
            OnPropertyChanged();
        }
    }
    private FontFamily _AyatNumberFontFamily;

    public FontFamily AyatNumberFontFamily
    {
        get { return _AyatNumberFontFamily; }
        set
        {
            _AyatNumberFontFamily = value;
            OnPropertyChanged();
        }
    }

    private double _AyatNumberFontSize;
    public double AyatNumberFontSize
    {
        get { return _AyatNumberFontSize; }
        set
        {
            _AyatNumberFontSize = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #endregion

    public ObservableCollection<QuranItem> QuranCollection { get; set; } = new ObservableCollection<QuranItem>();
    private List<TranslationItem> TranslationCollection { get; set; } = new List<TranslationItem>();
    private List<Quran> AyahCollection { get; set; } = new List<Quran>();

    internal static QuranTabViewItem Instance { get; private set; }

    private bool _IsTranslationAvailable = true;

    public bool IsTranslationAvailable
    {
        get { return _IsTranslationAvailable; }
        set
        {
            _IsTranslationAvailable = value;
            OnPropertyChanged();
        }
    }

    private bool _IsSurahTextAvailable = true;

    public bool IsSurahTextAvailable
    {
        get { return _IsSurahTextAvailable; }
        set
        {
            _IsSurahTextAvailable = value;
            OnPropertyChanged();
        }
    }

    public QuranTabViewItem()
    {
        this.InitializeComponent();
        Instance = this;
        DataContext = this;
        Loaded += QuranTabViewItem_LoadedAsync;
    }

    private void GetDefaultForeground()
    {
        if (Settings.AyatForeground is not null)
        {
            AyatForeground = new SolidColorBrush(ColorHelper.ToColor(Settings.AyatForeground));
        }
        if (Settings.AyatNumberForeground is not null)
        {
            AyatNumberForeground = new SolidColorBrush(ColorHelper.ToColor(Settings.AyatNumberForeground));
        }
        if (Settings.TranslationForeground is not null)
        {
            TranslationForeground = new SolidColorBrush(ColorHelper.ToColor(Settings.TranslationForeground));
        }
    }

    private void GetDefaultFont()
    {
        AyatFontFamily = new FontFamily(Settings.AyatFontFamilyName);
        AyatNumberFontFamily = new FontFamily(Settings.AyatNumberFontFamilyName);
        TranslationFontFamily = new FontFamily(Settings.TranslationFontFamilyName);

        AyatFontSize = Settings.AyatFontSize;
        AyatNumberFontSize = Settings.AyatNumberFontSize;
        TranslationFontSize = Settings.TranslationFontSize;
    }
    private void QuranTabViewItem_LoadedAsync(object sender, RoutedEventArgs e)
    {
        GetDefaultForeground();
        GetDefaultFont();

        GetSurahFromDB();
        GetTranslationText();
        GetSuraText();
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

    private async void GetSurahFromDB()
    {
        AyahCollection?.Clear();
        using var db = new AlAnvarDBContext();
        AyahCollection = await db.Qurans.Where(x => x.SurahId == SurahId).ToListAsync();
    }
    public void GetSuraText(bool isTranslationAvailable = true)
    {
        QuranCollection?.Clear();
       
        foreach (var item in AyahCollection)
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
                SurahName = SurahName,
                TotalAyah = TotalAyah,
                AyaDetail = $"({item.AyahNumber}:{TotalAyah})",
                TranslationText = isTranslationAvailable ? TranslationCollection.Where(x => x.Aya == item.AyahNumber).FirstOrDefault()?.Translation : null
            });
        }
        quranListView.ItemsSource = QuranCollection;
    }

    public void GetTranslationText()
    {
        TranslationCollection?.Clear();
        var selectedTranslation = Settings.QuranTranslation ?? QuranPage.Instance.GetComboboxFirstElement();
        if (Directory.Exists(Constants.TranslationsPath) && selectedTranslation is not null)
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

    private void quranListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var item = quranListView.SelectedItem as QuranItem;
        if (QuranPage.Instance is not null)
        {
            QuranPage.Instance.SetSurahStatus($"سوره: {item.SurahName} - آیه: {item.AyahNumber}");
        }
    }
}
