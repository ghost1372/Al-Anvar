using System.Text;

namespace AlAnvar.UI.TabViewItems;

public sealed partial class SubjectTabViewItem : TabViewItem, INotifyPropertyChanged
{
    public static readonly DependencyProperty SubjectProperty =
        DependencyProperty.Register("Subject", typeof(ExplorerItem), typeof(SubjectTabViewItem),
        new PropertyMetadata(null));

    public ExplorerItem Subject
    {
        get { return (ExplorerItem) GetValue(SubjectProperty); }
        set { SetValue(SubjectProperty, value); }
    }
    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

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

    public ObservableCollection<QuranItem> QuranCollection { get; set; } = new ObservableCollection<QuranItem>();
    private List<TranslationItem> TranslationCollection { get; set; } = new List<TranslationItem>();
    private List<Quran> AyahCollection { get; set; } = new List<Quran>();
    public SubjectTabViewItem()
    {
        this.InitializeComponent();
        DataContext = this;
        Loaded += SubjectTabViewItem_Loaded;
    }

    private async void SubjectTabViewItem_Loaded(object sender, RoutedEventArgs e)
    {
        await Task.Run(() => {
            GetDefaultForeground();
            GetDefaultFont();
            GetSurahFromDB();
            GetTranslationText();
            GetSuraText();
            LoadTranslationsInCombobox();
        });
        prgLoading.IsActive = false;
    }

    private void GetDefaultForeground()
    {
        DispatcherQueue.TryEnqueue(() =>
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
        });
    }
    private void GetDefaultFont()
    {
        DispatcherQueue.TryEnqueue(() =>
        {
            AyatFontFamily = new FontFamily(Settings.AyatFontFamilyName);
            AyatNumberFontFamily = new FontFamily(Settings.AyatNumberFontFamilyName);
            TranslationFontFamily = new FontFamily(Settings.TranslationFontFamilyName);

            AyatFontSize = Settings.AyatFontSize;
            AyatNumberFontSize = Settings.AyatNumberFontSize;
            TranslationFontSize = Settings.TranslationFontSize;
        });
    }

    private void GetSurahFromDB()
    {
        DispatcherQueue.TryEnqueue(async () =>
        {
            AyahCollection?.Clear();
            using var db = new AlAnvarDBContext();
            var verseIds = await db.Subjects.Where(x => x.SubjectId == Subject.SubjectId).Select(x => x.VerseId).ToListAsync();
            AyahCollection = await db.Qurans.Where(t => verseIds.Contains(t.Id)).ToListAsync();
        });
    }
    public void GetSuraText(bool isTranslationAvailable = true)
    {
        DispatcherQueue.TryEnqueue(() =>
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
                    SurahId = item.SurahId,
                    AyaDetail = $"({item.AyahNumber}:{item.SurahId})",
                    TranslationText = isTranslationAvailable ? TranslationCollection.Where(x => x.SurahId == item.SurahId && x.Aya == item.AyahNumber).FirstOrDefault()?.Translation : null
                });
            }
            quranListView.ItemsSource = QuranCollection;
        });
    }
    #region Translation
    public void GetTranslationText()
    {
        DispatcherQueue.TryEnqueue(() =>
        {
            TranslationCollection = new(GetQuranTranslation(cmbTranslators));
            if (TranslationCollection.Count == 0)
            {
                IsTranslationAvailable = false;
            }
        });
    }

    private void LoadTranslationsInCombobox()
    {
        if (Directory.Exists(Settings.TranslationsPath))
        {
            var items = new ObservableCollection<QuranTranslation>();
            var files = Directory.GetFiles(Settings.TranslationsPath, "*.ini", SearchOption.AllDirectories);
            if (files.Count() > 0)
            {
                foreach (var file in files)
                {
                    var trans = file.DeserializeFromJson<QuranTranslation>();
                    if (trans is not null)
                    {
                        items.Add(trans);
                    }
                }
                DispatcherQueue.TryEnqueue(() => {
                    cmbTranslators.ItemsSource = items;
                    cmbTranslators.SelectedItem = cmbTranslators.Items.Where(trans => ((QuranTranslation) trans).TranslationId == Settings.QuranTranslation?.TranslationId).FirstOrDefault();
                });
            }
        }
    }
    private void cmbTranslators_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        Settings.QuranTranslation = cmbTranslators.SelectedItem as QuranTranslation;

        var itemIndex = GetListViewSelectedIndex();
        TranslationCollection = new(GetQuranTranslation(cmbTranslators));
        GetSuraText();
        ScrollIntoView(itemIndex);
    }
    public int GetListViewSelectedIndex()
    {
        return quranListView.SelectedIndex;
    }
    public void ScrollIntoView(int index)
    {
        quranListView.SelectedIndex = index;
        quranListView.ScrollIntoView(quranListView.SelectedItem);
    }
    private void chkOnlyTranslationText_Checked(object sender, RoutedEventArgs e)
    {
        IsTranslationAvailable = !chkOnlyTranslationText.IsChecked.Value;
    }
    #endregion
    private void chkOnlyAyaText_Checked(object sender, RoutedEventArgs e)
    {
        IsSurahTextAvailable = !chkOnlyAyaText.IsChecked.Value;
    }
}
