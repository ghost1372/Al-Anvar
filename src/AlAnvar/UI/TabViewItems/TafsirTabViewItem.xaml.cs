namespace AlAnvar.UI.TabViewItems;
public sealed partial class TafsirTabViewItem : TabViewItem
{
    private int SurahId = 1;
    private int AyaId = 1;
    private List<TranslationItem> TranslationCollection { get; set; } = new List<TranslationItem>();
    private List<Quran> AyahCollection { get; set; } = new List<Quran>();
    private List<Tafsir> TafsirCollection { get; set; } = new List<Tafsir>();

    public TafsirTabViewItem()
    {
        this.InitializeComponent();
        Loaded += TafsirTabViewItem_Loaded;
    }

    private async void TafsirTabViewItem_Loaded(object sender, RoutedEventArgs e)
    {
        LoadTranslationsInCombobox();
        GetTranslationText();

        await Task.Run(() =>
        {
            GetSurahFromDB();
            LoadTafsirsInCombobox();
            GetTafsirText();
        });
        tafsirTreeView.ItemsSource = await GetData();

        prgLoading.IsActive = false;
    }
    private async Task<ObservableCollection<ExplorerItem>> GetData()
    {
        using var db = new AlAnvarDBContext();
        var chapters = await db.Chapters.ToListAsync();
        var list = new ObservableCollection<ExplorerItem>();

        foreach (var item in chapters)
        {
            ExplorerItem rootNode = new ExplorerItem() { Name = $"{item.Id}-{item.Name}", Type = ExplorerItem.ExplorerItemType.Folder };
            rootNode.IsExpanded = true;
            for (int i = 1; i <= item.Ayas; i++)
            {
                rootNode.Children.Add(new ExplorerItem()
                {
                    Name = $"آیه: {i}",
                    Type = ExplorerItem.ExplorerItemType.File,
                    Parent = rootNode
                });
            }
            list.Add(rootNode);
        }
        return list;
    }

    #region Tafsir
    private void cmbTafsir_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        Settings.QuranTafsir = cmbTafsir.SelectedItem as TafsirName;
        GetTafsirText();
        SetTafsirText();
    }

    private async void GetTafsirText()
    {
        TafsirCollection?.Clear();
        var selectedTafsir = Settings.QuranTafsir ?? GetTafsirComboboxFirstElement();
        if (selectedTafsir is not null)
        {
            using var db = new AlAnvarDBContext();
            var tafsir = await db.Tafsirs.Where(x => x.IdName == selectedTafsir.Id).ToListAsync();
            TafsirCollection = new(tafsir);
        }
    }
    public TafsirName GetTafsirComboboxFirstElement()
    {
        cmbTafsir.SelectedIndex = 0;
        return cmbTafsir.SelectedItem as TafsirName;
    }
    public void SetTafsirText()
    {
        var id = AyahCollection?.Where(x => x.SurahId == SurahId && x.AyahNumber == AyaId)?.FirstOrDefault()?.Id;
        var tafsir = TafsirCollection?.Where(x => x.IdVerse.Contains($"[{id}]"))?.FirstOrDefault()?.Value;
        txtTafsir.Document.SetText(Microsoft.UI.Text.TextSetOptions.None, tafsir);
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
    #endregion

    #region Translation
    private void cmbTranslators_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        Settings.QuranTranslation = cmbTranslators.SelectedItem as QuranTranslation;
        GetTranslationText();
        SetAyaOrTranslationText();
    }
    public QuranTranslation GetTranslatorComboboxFirstElement()
    {
        cmbTranslators.SelectedIndex = 0;
        return cmbTranslators.SelectedItem as QuranTranslation;
    }
    public void SetAyaOrTranslationText()
    {
        if (radioButtons.SelectedIndex == 0)
        {
            txtAya.Text = AyahCollection?.Where(x => x.SurahId == SurahId && x.AyahNumber == AyaId)?.FirstOrDefault()?.AyahText;
        }
        else
        {
            txtAya.Text = TranslationCollection?.Where(x => x.SurahId == SurahId && x.Aya == AyaId)?.FirstOrDefault()?.Translation;
        }
    }


    public void GetTranslationText()
    {
        TranslationCollection?.Clear();
        var selectedTranslation = Settings.QuranTranslation ?? GetTranslatorComboboxFirstElement();
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
    }
    private void LoadTranslationsInCombobox()
    {
        if (Directory.Exists(Settings.TranslationsPath))
        {
            var files = Directory.GetFiles(Settings.TranslationsPath, "*.ini", SearchOption.AllDirectories);
            if (files.Count() > 0)
            {
                foreach (var file in files)
                {
                    var trans = JsonConvert.DeserializeObject<QuranTranslation>(File.ReadAllText(file));
                    if (trans is not null)
                    {
                        cmbTranslators.Items.Add(trans);
                    }
                }
            }
            cmbTranslators.SelectedItem = cmbTranslators.Items.Where(trans => ((QuranTranslation) trans).TranslationId == Settings.QuranTranslation?.TranslationId)?.FirstOrDefault();
        }
    }

    #endregion
    private void TreeViewItem_Tapped(object sender, TappedRoutedEventArgs e)
    {
        var treeViewItem = (sender as TreeViewItem).DataContext as ExplorerItem;
        var ayaId = Convert.ToInt32(treeViewItem.Name.Replace("آیه: ", ""));

        var parent = treeViewItem.Parent.Name;
        var surahId = Convert.ToInt32(parent.Substring(0, parent.IndexOf("-")));
        SurahId = surahId;
        AyaId = ayaId;

        SetAyaOrTranslationText();
        SetTafsirText();
    }
    
    private async void GetSurahFromDB()
    {
        AyahCollection?.Clear();
        using var db = new AlAnvarDBContext();
        AyahCollection = await db.Qurans.ToListAsync();
    }
    
    private void RadioButtons_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        SetAyaOrTranslationText();
    }
}
