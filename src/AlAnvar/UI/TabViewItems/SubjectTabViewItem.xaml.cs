using System.Text;

namespace AlAnvar.UI.TabViewItems;

public sealed partial class SubjectTabViewItem : TabViewItem
{
    public static readonly DependencyProperty SubjectProperty =
        DependencyProperty.Register("Subject", typeof(ExplorerItem), typeof(SubjectTabViewItem),
        new PropertyMetadata(null));

    public ExplorerItem Subject
    {
        get { return (ExplorerItem) GetValue(SubjectProperty); }
        set { SetValue(SubjectProperty, value); }
    }
    public SubjectTabViewItem()
    {
        this.InitializeComponent();
        Loaded += SubjectTabViewItem_Loaded;
    }

    private async void SubjectTabViewItem_Loaded(object sender, RoutedEventArgs e)
    {
        using var db = new AlAnvarDBContext();
        var verseId = await db.Subjects.Where(x => x.SubjectId == Subject.SubjectId).Select(x=>x.VerseId).ToListAsync();
        var aya = await db.Qurans.Where(t => verseId.Contains(t.Id)).ToListAsync();

        StringBuilder stringBuilder = new StringBuilder();

        foreach (var item in aya)
        {
            stringBuilder.AppendLine(item.AyahText);
        }

        txtSubject.Document.SetText(Microsoft.UI.Text.TextSetOptions.None, stringBuilder.ToString());
    }
}
