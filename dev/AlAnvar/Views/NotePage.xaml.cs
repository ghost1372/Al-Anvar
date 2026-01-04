using AlAnvar.Database;
using AlAnvar.Database.Tables;
using AlAnvar.Models;
using Microsoft.EntityFrameworkCore;

namespace AlAnvar.Views;

public sealed partial class NotePage : Page
{
    public NoteViewModel ViewModel { get; }
    private bool isNewNote = false;
    public NotePage()
    {
        ViewModel = App.GetService<NoteViewModel>();
        InitializeComponent();

        Loaded -= OnLoaded;
        Loaded += OnLoaded;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        await ViewModel.OnPageLoaded();
    }
    
    private async void OnAdd(object sender, RoutedEventArgs e)
    {
        isNewNote = true;

        TxtTitle.Text = string.Empty;
        TxtNote.Text = string.Empty;
        CreatedShield.Status = string.Empty;
        UpdatedShield.Status = string.Empty;

        BtnSave.IsEnabled = true;
        CmbQuranSura.IsEnabled = true;
        CmbQuranVerse.IsEnabled = true;
        TxtNote.IsEnabled = true;
        TxtTitle.IsEnabled = true;
        BtnAdd.IsEnabled = false;
        BtnDelete.IsEnabled = false;

        NoteTreeView.SelectedNode = null;
        NoteTreeView.SelectedItems?.Clear();
    }
    private async void OnSave(object sender, RoutedEventArgs e)
    {
        ViewModel.IsActive = true;
        using var db = new AlAnvarDBContext();

        if (CmbQuranVerse.SelectedIndex == -1 || CmbQuranSura.SelectedIndex == -1)
        {
            await MessageBox.ShowWarningAsync(Strings.NotePage_AddNoteNoSelectedError.GetLocalizedResource(), Strings.NotePage_AddNoteNoSelectedErrorTitle.GetLocalizedResource());
            return;
        }

        if (string.IsNullOrEmpty(TxtTitle.Text) || string.IsNullOrEmpty(TxtNote.Text))
        {
            await MessageBox.ShowWarningAsync(Strings.NotePage_AddNoteEmptyError.GetLocalizedResource(), Strings.NotePage_AddNoteEmptyErrorTitle.GetLocalizedResource());
            return;
        }

        if (isNewNote)
        {
            isNewNote = false;
            if (CmbQuranVerse.SelectedItem is FinalQuran finalQuran)
            {
                try
                {
                    var note = new QuranNoteTable
                    {
                        AyaId = finalQuran.AyaId,
                        SuraId = finalQuran.SuraId,
                        Title = TxtTitle.Text,
                        Description = TxtNote.Text,
                        CreatedAt = DateTime.Now.ToShortDateString(),
                        UpdatedAt = DateTime.Now.ToShortDateString()
                    };

                    var res = await db.Notes.AddAsync(note);
                    await db.SaveChangesAsync();

                    var cachedSuraIndex = CmbQuranSura.SelectedIndex;
                    var cachedVerseIndex = CmbQuranVerse.SelectedIndex;

                    await ViewModel.OnPageLoaded();

                    CmbQuranSura.SelectedIndex = cachedSuraIndex;
                    CmbQuranVerse.SelectedIndex = cachedVerseIndex;

                    NoteInfoBar.Title = Strings.NotePage_InfoBarAddSuccess_Title.GetLocalizedResource();
                    NoteInfoBar.Message = Strings.NotePage_InfoBarAddSuccess_Message.GetLocalizedResource();
                    NoteInfoBar.Severity = InfoBarSeverity.Success;
                    NoteInfoBar.IsOpen = true;

                    TxtTitle.Text = string.Empty;
                    TxtNote.Text = string.Empty;
                    CreatedShield.Status = string.Empty;
                    UpdatedShield.Status = string.Empty;

                    BtnSave.IsEnabled = false;
                    CmbQuranSura.IsEnabled = false;
                    CmbQuranVerse.IsEnabled = false;
                    TxtNote.IsEnabled = false;
                    TxtTitle.IsEnabled = false;
                    BtnAdd.IsEnabled = true;
                    BtnDelete.IsEnabled = false;
                }
                catch (Exception ex)
                {
                    NoteInfoBar.Title = Strings.NotePage_InfoBarAddError_Title.GetLocalizedResource();
                    NoteInfoBar.Message = Strings.NotePage_InfoBarAddError_Message.GetLocalizedResource();
                    NoteInfoBar.Severity = InfoBarSeverity.Error;
                    NoteInfoBar.IsOpen = true;
                    Logger?.Error(ex, ex.Message);
                    await MessageBox.ShowErrorAsync(ex.Message, Strings.MessageBoxErrorTitle.GetLocalizedResource());
                }
            }
        }
        else
        {
            if (CmbQuranVerse.SelectedItem is FinalQuran finalQuran)
            {
                try
                {
                    if (NoteTreeView.SelectedNode != null && NoteTreeView.SelectedNode.Content is NoteNode node)
                    {
                        var note = await Queries.GetNoteByIdQueryAsync(db, node.NoteId).FirstOrDefaultAsync();
                        if (note != null)
                        {
                            note.Title = TxtNote.Text;
                            note.Description = TxtNote.Text;
                            note.SuraId = finalQuran.SuraId;
                            note.AyaId = finalQuran.AyaId;
                            note.UpdatedAt = DateTime.Now.ToShortDateString();
                            await db.SaveChangesAsync();

                            NoteInfoBar.Title = Strings.NotePage_InfoBarUpdateSuccess_Title.GetLocalizedResource();
                            NoteInfoBar.Message = Strings.NotePage_InfoBarUpdateSuccess_Message.GetLocalizedResource();
                            NoteInfoBar.Severity = InfoBarSeverity.Success;
                            NoteInfoBar.IsOpen = true;
                        }
                    }
                    
                }
                catch (Exception ex)
                {
                    NoteInfoBar.Title = Strings.NotePage_InfoBarUpdateError_Title.GetLocalizedResource();
                    NoteInfoBar.Message = Strings.NotePage_InfoBarUpdateError_Message.GetLocalizedResource();
                    NoteInfoBar.Severity = InfoBarSeverity.Error;
                    NoteInfoBar.IsOpen = true;
                    ViewModel.IsActive = false;
                    Logger?.Error(ex, ex.Message);
                    await MessageBox.ShowErrorAsync(ex.Message, Strings.MessageBoxErrorTitle.GetLocalizedResource());
                }
            }
        }

        ViewModel.IsActive = false;
    }
    private async void OnDelete(object sender, RoutedEventArgs e)
    {
        ViewModel.IsActive = true;

        using var db = new AlAnvarDBContext();

        if (NoteTreeView.SelectedNode == null)
            return;

        try
        {
            if (NoteTreeView.SelectedNode.Content is NoteNode node)
            {
                var note = await Queries.GetNoteByIdQueryAsync(db, node.NoteId).FirstOrDefaultAsync();
                if (note != null)
                {
                    db.Notes.Remove(note);
                    await db.SaveChangesAsync();
                }

            }
            else if (NoteTreeView.SelectedNode.Content is AyaNode ayaNode)
            {
                var noteIds = ayaNode.Notes.Select(n => n.NoteId).ToList();
                var result = await Queries.GetNotesByIdsAsync(db, noteIds).ToListAsync();
                if (result != null)
                {
                    db.Notes.RemoveRange(result);
                    await db.SaveChangesAsync();
                }
            }
            else if (NoteTreeView.SelectedNode.Content is SuraNode suraNode)
            {
                var noteIds = suraNode.Ayas.SelectMany(a => a.Notes).Select(n => n.NoteId).ToList();
                var result = await Queries.GetNotesByIdsAsync(db, noteIds).ToListAsync();
                if (result != null)
                {
                    db.Notes.RemoveRange(result);
                    await db.SaveChangesAsync();
                }
            }

            NoteInfoBar.Title = Strings.NotePage_InfoBarDeleteSuccess_Title.GetLocalizedResource();
            NoteInfoBar.Message = Strings.NotePage_InfoBarDeleteSuccess_Message.GetLocalizedResource();
            NoteInfoBar.Severity = InfoBarSeverity.Success;
            NoteInfoBar.IsOpen = true;

            TxtNote.Text = string.Empty;
            TxtTitle.Text = string.Empty;
            CreatedShield.Status = string.Empty;
            UpdatedShield.Status = string.Empty;
            BtnDelete.IsEnabled = false;
            BtnSave.IsEnabled = false;

            CmbQuranSura.IsEnabled = false;
            CmbQuranVerse.IsEnabled = false;
            TxtNote.IsEnabled = false;
            TxtTitle.IsEnabled = false;
            BtnAdd.IsEnabled = true;

            CmbQuranSura.SelectedIndex = -1;
            CmbQuranVerse.SelectedIndex = -1;

            NoteTreeView.SelectedNode = null;
            NoteTreeView.SelectedItems?.Clear();

            await ViewModel.OnPageLoaded();
        }
        catch (Exception ex)
        {
            NoteInfoBar.Title = Strings.NotePage_InfoBarDeleteError_Title.GetLocalizedResource();
            NoteInfoBar.Message = Strings.NotePage_InfoBarDeleteError_Message.GetLocalizedResource();
            NoteInfoBar.Severity = InfoBarSeverity.Error;
            NoteInfoBar.IsOpen = true;
            ViewModel.IsActive = false;
            Logger?.Error(ex, ex.Message);
            await MessageBox.ShowErrorAsync(ex.Message, Strings.MessageBoxErrorTitle.GetLocalizedResource());
        }

        ViewModel.IsActive = false;
    }

    private void TreeView_SelectionChanged(TreeView sender, TreeViewSelectionChangedEventArgs args)
    {
        if (sender.SelectedNode == null)
            return;

        isNewNote = false;
        NoteInfoBar.IsOpen = false;
        BtnDelete.IsEnabled = true;
        BtnAdd.IsEnabled = true;

        if (sender.SelectedNode.Content is SuraNode node)
        {
            TxtTitle.Text = string.Empty;
            TxtNote.Text = string.Empty;
            CreatedShield.Status = string.Empty;
            UpdatedShield.Status = string.Empty;

            CmbQuranSura.IsEnabled = false;
            CmbQuranVerse.IsEnabled = false;
            TxtNote.IsEnabled = false;
            TxtTitle.IsEnabled = false;
            BtnSave.IsEnabled = false;

            var item = CmbQuranSura.Items.OfType<QuranMetadataTable>().Where(x => x.Id == node.SuraId).FirstOrDefault();
            CmbQuranSura.SelectedItem = item;

            CmbQuranVerse.SelectedIndex = -1;
        }
        else if (sender.SelectedNode.Content is AyaNode ayaNode)
        {
            TxtTitle.Text = string.Empty;
            TxtNote.Text = string.Empty;
            CreatedShield.Status = string.Empty;
            UpdatedShield.Status = string.Empty;

            CmbQuranSura.IsEnabled = false;
            CmbQuranVerse.IsEnabled = false;
            TxtNote.IsEnabled = false;
            TxtTitle.IsEnabled = false;
            BtnSave.IsEnabled = false;

            var item = CmbQuranSura.Items.OfType<QuranMetadataTable>().Where(x => x.Id == ayaNode.SuraId).FirstOrDefault();
            CmbQuranSura.SelectedItem = item;

            var verseItem = CmbQuranVerse.Items.OfType<FinalQuran>().Where(x => x.AyaId == ayaNode.AyaId).FirstOrDefault();
            CmbQuranVerse.SelectedItem = verseItem;
        }
        else if (sender.SelectedNode.Content is NoteNode noteNode)
        {
            TxtTitle.Text = noteNode.Title;
            TxtNote.Text = noteNode.Description;
            CreatedShield.Status = noteNode.CreatedAt;
            UpdatedShield.Status = noteNode.UpdatedAt;

            CmbQuranSura.IsEnabled = true;
            CmbQuranVerse.IsEnabled = true;
            TxtNote.IsEnabled = true;
            TxtTitle.IsEnabled = true;
            BtnSave.IsEnabled = true;

            var item = CmbQuranSura.Items.OfType<QuranMetadataTable>().Where(x => x.Id == noteNode.SuraId).FirstOrDefault();
            CmbQuranSura.SelectedItem = item;

            var verseItem = CmbQuranVerse.Items.OfType<FinalQuran>().Where(x => x.AyaId == noteNode.AyaId).FirstOrDefault();
            CmbQuranVerse.SelectedItem = verseItem;
        }
        else
        {
            TxtTitle.Text = string.Empty;
            TxtNote.Text = string.Empty;
            CreatedShield.Status = string.Empty;
            UpdatedShield.Status = string.Empty;

            CmbQuranSura.IsEnabled = false;
            CmbQuranVerse.IsEnabled = false;
            TxtNote.IsEnabled = false;
            TxtTitle.IsEnabled = false;
            BtnSave.IsEnabled = false;
            BtnDelete.IsEnabled = false;
        }
    }

    private async void CmbQuranSura_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (CmbQuranSura.SelectedIndex != -1 && CmbQuranSura.SelectedItem is QuranMetadataTable quranMetadata)
        {
            await ViewModel.GetQuranVerse(quranMetadata);
        }
    }
}
