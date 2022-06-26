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
    }
}
