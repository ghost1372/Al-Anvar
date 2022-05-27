using Windows.ApplicationModel.DataTransfer;

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

    #endregion

    public ObservableCollection<QuranItem> QuranCollection { get; set; } = new ObservableCollection<QuranItem>();
    private List<TranslationItem> TranslationCollection { get; set; } = new List<TranslationItem>();
    private List<Quran> AyahCollection { get; set; } = new List<Quran>();

    public QuranTabViewItem()
    {
        this.InitializeComponent();
        DataContext = this;
        Loaded += QuranTabViewItem_Loaded;
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
    private async void QuranTabViewItem_Loaded(object sender, RoutedEventArgs e)
    {
        await Task.Run(() => {
            GetDefaultForeground();
            GetDefaultFont();
            GetSurahFromDB();
            GetTranslationText();
            GetSuraText();
        });
    }

    private void GetSurahFromDB()
    {
        DispatcherQueue.TryEnqueue(async () =>
        {
            AyahCollection?.Clear();
            using var db = new AlAnvarDBContext();
            AyahCollection = await db.Qurans.Where(x => x.SurahId == SurahId).ToListAsync();
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
                    SurahId = SurahId,
                    SurahName = SurahName,
                    TotalAyah = TotalAyah,
                    AyaDetail = $"({item.AyahNumber}:{TotalAyah})",
                    TranslationText = isTranslationAvailable ? TranslationCollection.Where(x => x.Aya == item.AyahNumber).FirstOrDefault()?.Translation : null
                });
            }
            quranListView.ItemsSource = QuranCollection;
        });
    }

    public void GetTranslationText()
    {
        DispatcherQueue.TryEnqueue(() =>
        {
            TranslationCollection?.Clear();
            var selectedTranslation = Settings.QuranTranslation ?? QuranPage.Instance.GetComboboxFirstElement();
            if (Directory.Exists(Settings.TranslationsPath) && selectedTranslation is not null)
            {
                var files = Directory.GetFiles(Settings.TranslationsPath, "*.txt", SearchOption.AllDirectories);
                if (files.Count() > 0)
                {
                    foreach (var item in files)
                    {
                        if (Path.GetFileNameWithoutExtension(item).Equals(selectedTranslation.TranslationId))
                        {
                            using (var streamReader = File.OpenText(item))
                            {
                                string line = String.Empty;
                                while ((line = streamReader.ReadLine()) != null)
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
        });
    }

    private void quranListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var item = quranListView.SelectedItem as QuranItem;
        if (QuranPage.Instance is not null && item is not null)
        {
            QuranPage.Instance.SetSurahStatus($"سوره: {item.SurahName} - آیه: {item.AyahNumber}");
            QuranPage.Instance.UpdateMediaPlayerButtons(quranListView.SelectedIndex, quranListView.Items.Count - 1);
        }
        ScrollIntoView(quranListView.SelectedIndex);
    }

    public QuranItem GetListViewSelectedItem()
    {
        return quranListView.SelectedItem as QuranItem;
    }

    public void GoToListViewNextItem()
    {
        var currentIndex = quranListView.SelectedIndex;
        if (currentIndex == -1)
        {
            quranListView.SelectedIndex = 0;
        }
        else if (currentIndex != quranListView.Items.Count -1)
        {
            quranListView.SelectedIndex += 1;
        } 
    }

    public int GetListViewLastIndex()
    {
        return quranListView.Items.Count - 1;
    }
    public int GetListViewSelectedIndex()
    {
        return quranListView.SelectedIndex;
    }
    public void GoToListViewPreviousItem()
    {
        var currentIndex = quranListView.SelectedIndex;
        if (currentIndex == -1)
        {
            quranListView.SelectedIndex = 0;
        }
        else if (currentIndex != 0)
        {
            quranListView.SelectedIndex -= 1;
        }
    }
    public void ScrollIntoView(int index)
    {
        quranListView.SelectedIndex = index;
        quranListView.ScrollIntoView(quranListView.SelectedItem);
    }

    private void menuFlyout_Click(object sender, RoutedEventArgs e)
    {
        var selectedItem = quranListView.SelectedItem as QuranItem;
        DataPackage dataPackage = new DataPackage();
        dataPackage.RequestedOperation = DataPackageOperation.Copy;
        switch ((sender as MenuFlyoutItem).Tag)
        {
            case "Play":
                QuranPage.Instance.CallBtnPlay();
                break;
            case "CopyTranslation":
                dataPackage.SetText(selectedItem.TranslationText);
                Clipboard.SetContent(dataPackage);
                break;
            case "CopyAya":
                dataPackage.SetText(selectedItem.AyahText);
                Clipboard.SetContent(dataPackage);
                break;
            case "Tafsir":

                break;
        }
    }

    private void quranListView_RightTapped(object sender, Microsoft.UI.Xaml.Input.RightTappedRoutedEventArgs e)
    {
        menuFlyout.ShowAt(quranListView, e.GetPosition(quranListView));
    }
}
