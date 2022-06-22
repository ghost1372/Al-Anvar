using Microsoft.UI.Xaml.Documents;
using Windows.Graphics.Printing;
using PrintHelper = AlAnvar.Common.PrintHelper;

namespace AlAnvar.UI.TabViewItems;
public sealed partial class TafsirTabViewItem : TabViewItem
{
    private int SurahId = 1;
    private int AyaId = 1;
    private List<TranslationItem> TranslationCollection { get; set; } = new List<TranslationItem>();
    private List<Quran> AyahCollection { get; set; } = new List<Quran>();
    private List<Tafsir> TafsirCollection { get; set; } = new List<Tafsir>();
    private List<ChapterProperty> ChapterCollection { get; set; } = new List<ChapterProperty>();
    private PrintHelper printHelper;

    internal static TafsirTabViewItem Instance { get; set; }

    public TafsirTabViewItem()
    {
        this.InitializeComponent();
        Instance = this;
        Loaded += TafsirTabViewItem_Loaded;
    }

    private async void TafsirTabViewItem_Loaded(object sender, RoutedEventArgs e)
    {
        LoadTranslationsInCombobox();
        GetTranslationText();

        await Task.Run(() =>
        {
            GetChapterProeprty();
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

    private async void GetChapterProeprty()
    {
        using var db = new AlAnvarDBContext();
        ChapterCollection = await db.Chapters.ToListAsync();
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
        TafsirName selectedTafsir = null;
        if (DispatcherQueue.HasThreadAccess)
        {
            selectedTafsir = Settings.QuranTafsir ?? GetComboboxFirstElement<TafsirName>(cmbTafsir);
        }

        if (selectedTafsir is not null)
        {
            using var db = new AlAnvarDBContext();
            var tafsir = await db.Tafsirs.Where(x => x.IdName == selectedTafsir.Id).ToListAsync();
            TafsirCollection = new(tafsir);
        }
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
        txtAya.Header = GetSuraDetail(SurahId);
    }

    public void GetTranslationText()
    {
        TranslationCollection = new(GetQuranTranslation(cmbTranslators));
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
                    var trans = file.DeserializeFromJson<QuranTranslation>();
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
    private string GetSuraDetail(int surahId)
    {
        var chapter = ChapterCollection?.Where(x => x.Id == surahId)?.FirstOrDefault();
        return $"{chapter?.Name} {AyaId}:{chapter?.Ayas}";
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

    #region Print

    private void RegisterPrint()
    {
        printHelper = new PrintHelper(WindowHelper.GetWindowHandleForCurrentWindow(MainWindow.Instance), this,
            "نرم افزار الانوار", "Print Tafsir");
        printHelper.OnPrintSucceeded += PrintHelper_OnPrintSucceeded;
        printHelper.OnPrintFailed += PrintHelper_OnPrintFailed;
        printHelper.OnPrintCanceled += PrintHelper_OnPrintCanceled;
        printHelper.RegisterForPrinting();
    }

    private void PrintHelper_OnPrintCanceled()
    {
        UnRegister();
        MainWindow.Instance.SetMainGridFlowDirection(FlowDirection.RightToLeft);
    }

    private void UnRegister()
    {
        if (printHelper != null)
        {
            printHelper.UnregisterForPrinting();
        }
    }
    public async void Print()
    {
        if (PrintManager.IsSupported())
        {
            MainWindow.Instance.SetMainGridFlowDirection(FlowDirection.LeftToRight);
            RegisterPrint();
            Paragraph paragraph = new Paragraph();
            Run run = new Run();
            txtTafsir.Document.GetText(Microsoft.UI.Text.TextGetOptions.None, out string tafsirContent);
            run.Text = tafsirContent;
            paragraph.Inlines.Add(run);
            printHelper.PreparePrintContent(new PageToPrint("تفسیر",
                new string[] { txtAya.Header?.ToString(), txtAya.Text, ((TafsirName) cmbTafsir.SelectedItem)?.Name },
                "نرم افزار الانوار", paragraph));
            await printHelper.ShowPrintUIAsync();
        }
        else
        {
            // Printing is not supported on this device
            ContentDialog noPrintingDialog = new ContentDialog()
            {
                XamlRoot = this.XamlRoot,
                Title = "عدم پشتیبانی از پرینت",
                Content = "\nمتاسفانه دستگاه شما از پرینت پشتیبانی نمی کند",
                PrimaryButtonText = "تایید"
            };
            await noPrintingDialog.ShowAsyncQueue();
        }
    }

    private async void PrintHelper_OnPrintFailed()
    {
        MainWindow.Instance.SetMainGridFlowDirection(FlowDirection.RightToLeft);

        UnRegister();
        ContentDialog noPrintingDialog = new ContentDialog()
        {
            XamlRoot = this.XamlRoot,
            Title = "خطای پرینت",
            Content = "\nمتاسفانه پرینت با خطا مواجه شد",
            PrimaryButtonText = "تایید"
        };
        await noPrintingDialog.ShowAsyncQueue();
    }

    private async void PrintHelper_OnPrintSucceeded()
    {
        MainWindow.Instance.SetMainGridFlowDirection(FlowDirection.RightToLeft);

        UnRegister();
        ContentDialog noPrintingDialog = new ContentDialog()
        {
            XamlRoot = this.XamlRoot,
            Title = "پرینت موفقیت آمیز",
            Content = "\nپرینت صفحات با موفقیت انجام شد",
            PrimaryButtonText = "تایید"
        };
        await noPrintingDialog.ShowAsyncQueue();
    }
    #endregion

}
