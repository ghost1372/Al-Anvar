using AlAnvar.Database;
using AlAnvar.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.UI.Xaml.Documents;

namespace AlAnvar.Views;

public sealed partial class TafsirTabViewItem : TabViewItem
{
    public FinalQuran FinalQuran { get; set; }
    public TafsirType TafsirType { get; set; }
    private string paragraph;

    public TafsirTabViewItemViewModel ViewModel { get; }
    internal static TafsirTabViewItem Instance { get; private set; }
    public TafsirTabViewItem()
    {
        ViewModel = App.GetService<TafsirTabViewItemViewModel>();

        InitializeComponent();

        Instance = this;

        Loaded -= OnLoaded;
        Loaded += OnLoaded;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        ViewModel.IsActive = true;

        await Task.Run(async() =>
        {
            try
            {
                using var db = new AlAnvarDBContext();
                var selectedQuranId = await Queries.GetQuranByIdQueryAsync(db, FinalQuran.SuraId, FinalQuran.AyaId).Select(x => x.Id).FirstOrDefaultAsync();
                int selectedExplanationId = 0;
                switch (TafsirType)
                {
                    case TafsirType.AlMizan:
                        selectedExplanationId = 1;
                        break;
                    case TafsirType.Nemone:
                        selectedExplanationId = 2;
                        break;
                }

                var result = await Queries.GetTafsirByIdQueryAsync(db, selectedExplanationId).Where(x => VerseContains(x.VerseIds, selectedQuranId)).ToListAsync();

                DispatcherQueue.TryEnqueue(() =>
                {
                    TafsirRichTextBlock.Blocks.Clear();

                    var paragraph = new Paragraph();

                    foreach (var item in result)
                    {
                        paragraph.Inlines.Add(new Run { Text = item.Description });
                        paragraph.Inlines.Add(new LineBreak());
                    }

                    this.paragraph = paragraph.ToString();
                    TafsirRichTextBlock.Blocks.Add(paragraph);
                });
            }
            catch (Exception ex)
            {
                ViewModel.IsActive = false;
                Logger?.Error(ex, ex.Message);
                DispatcherQueue.TryEnqueue(async () =>
                {
                    await MessageBox.ShowErrorAsync(ex.Message, Strings.MessageBoxErrorTitle.GetLocalizedResource());
                });
            }
        });

        ViewModel.IsActive = false;
    }

    public async void OnMenuClicked(object sender, RoutedEventArgs e)
    {
        CopyToClipboard(paragraph);
    }
}
