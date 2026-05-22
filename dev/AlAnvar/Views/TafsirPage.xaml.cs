using System.Text;
using AlAnvar.Database;
using AlAnvar.Database.Tables;
using AlAnvar.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.UI.Xaml.Documents;

namespace AlAnvar.Views;

public sealed partial class TafsirPage : Page
{
    public TafsirViewModel ViewModel { get; }
    private string paragraph;
    public TafsirPage()
    {
        ViewModel = App.GetService<TafsirViewModel>();
        InitializeComponent();

        DataContext = ViewModel;

        Loaded -= OnLoaded;
        Loaded += OnLoaded;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        await ViewModel.OnPageLoaded();
    }

    private async void TafsirTreeView_SelectionChanged(TreeView sender, TreeViewSelectionChangedEventArgs args)
    {
        if (sender.SelectedNode.Content is AyaNode ayaNode)
        {
            await GetTafsir(ayaNode);
        }
    }

    private async Task GetTafsir(AyaNode ayaNode)
    {
        ViewModel.IsActive = true;

        int selectedExplanationId = 0;

        if (CmbTafsir.SelectedItem is QuranTafsirNameTable quranTafsirName)
        {
            selectedExplanationId = quranTafsirName.Id;
        }

        await Task.Run(async () =>
        {
            try
            {
                using var db = new AlAnvarDBContext();
                var selectedQuranId = await Queries.GetAllQuranQueryAsync(db).Where(x => x.SuraId == ayaNode.SuraId && x.AyaId == ayaNode.AyaId).Select(x => x.Id).FirstOrDefaultAsync();
                var tafsirs = await Queries.GetTafsirByIdQueryAsync(db, selectedExplanationId)
                    .ToListAsync();

                var result = tafsirs
                    .Where(x => VerseContains(x.VerseIds, selectedQuranId))
                    .ToList();

                DispatcherQueue.TryEnqueue(async() =>
                {
                    var tafsirParagraph = new Paragraph();
                    StringBuilder stringBuilder = new StringBuilder();
                    foreach (var item in result)
                    {
                        stringBuilder.AppendLine(item.Description);
                        tafsirParagraph.Inlines.Add(new Run { Text = item.Description });
                        tafsirParagraph.Inlines.Add(new LineBreak());
                    }

                    TafsirRichTextBlock.Blocks.Clear();
                    TafsirRichTextBlock.Blocks.Add(tafsirParagraph);

                    this.paragraph = stringBuilder.ToString();
                });
            }
            catch (Exception ex)
            {
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
