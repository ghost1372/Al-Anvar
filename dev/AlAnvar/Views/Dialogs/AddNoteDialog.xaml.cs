using AlAnvar.Database;
using AlAnvar.Database.Tables;
using AlAnvar.Models;

namespace AlAnvar.Views;

public sealed partial class AddNoteDialog : ContentDialogWindow
{
    public QuranViewModel ViewModel { get; }
    public AddNoteDialog(QuranViewModel quranViewModel)
    {
        InitializeComponent();
        ViewModel = quranViewModel;

        Owner = App.MainWindow;
    }

    private async void OnDialogPrimaryButtonClick(object sender, EventArgs args)
    {
        try
        {
            using var db = new AlAnvarDBContext();
            int noteSuraId = -1;
            int noteVerseId = -1;

            if (CmbQuranVerse.SelectedItem is FinalQuran finalQuran)
            {
                noteSuraId = finalQuran.SuraId;
                noteVerseId = finalQuran.AyaId;
            }

            if (noteSuraId == -1 || noteVerseId == -1)
            {
                await MessageBox.ShowErrorAsync(Strings.QuranPage_DialogAddNoteError.GetLocalizedResource(), Strings.MessageBoxErrorTitle.GetLocalizedResource());
                return;
            }

            if (string.IsNullOrEmpty(ViewModel.TitleNote) || string.IsNullOrEmpty(ViewModel.DescriptionNote))
            {
                await MessageBox.ShowErrorAsync(Strings.QuranPage_DialogAddNoteEmptyError.GetLocalizedResource(), Strings.MessageBoxErrorTitle.GetLocalizedResource());
                return;
            }

            var note = new QuranNoteTable
            {
                SuraId = noteSuraId,
                AyaId = noteVerseId,
                Title = ViewModel.TitleNote,
                Description = ViewModel.DescriptionNote,
                CreatedAt = DateTime.Now.ToShortDateString(),
                UpdatedAt = DateTime.Now.ToShortDateString(),
            };
            await db.Notes.AddAsync(note);
            await db.SaveChangesAsync();
            var dialog = sender as ContentDialogWindow;
            dialog?.TryClose();
        }
        catch (Exception ex)
        {
            Logger?.Error(ex, ex.Message);
            await MessageBox.ShowErrorAsync(ex.Message, Strings.MessageBoxErrorTitle.GetLocalizedResource());
        }
    }

    private async void CmbQuranSura_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (CmbQuranSura.SelectedItem is QuranMetadataTable quranMetadata)
        {
            await ViewModel.GetQuranVerse(quranMetadata);
        }
    }
}
