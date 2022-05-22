using System.Runtime.CompilerServices;

namespace AlAnvar.UI.UserControls;

public sealed partial class QuranTabViewItem : TabViewItem, INotifyPropertyChanged
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

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

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

    private void QuranTabViewItem_LoadedAsync(object sender, RoutedEventArgs e)
    {
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
}
