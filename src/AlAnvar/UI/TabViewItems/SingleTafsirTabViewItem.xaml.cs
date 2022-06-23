using Microsoft.UI.Xaml.Documents;
using Windows.Graphics.Printing;
using PrintHelper = AlAnvar.Common.PrintHelper;

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

    private PrintHelper printHelper;
    internal static SingleTafsirTabViewItem Instance;
    public SingleTafsirTabViewItem()
    {
        this.InitializeComponent();
        Instance = this;
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

    #region Print

    private void RegisterPrint()
    {
        printHelper = new PrintHelper(WindowHelper.GetWindowHandleForCurrentWindow(MainWindow.Instance), this,
            "نرم افزار الانوار", "Print Single Tafsir");
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
