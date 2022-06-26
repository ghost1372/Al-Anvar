namespace AlAnvar.Common;
public class ExplorerItem : Observable
{
    public enum ExplorerItemType { Folder, File, CheckMark };
    public string Name { get; set; }
    public ExplorerItem Parent { get; set; }
    public ExplorerItemType Type { get; set; }

    private ObservableCollection<ExplorerItem> m_children;
    public ObservableCollection<ExplorerItem> Children
    {
        get
        {
            if (m_children == null)
            {
                m_children = new ObservableCollection<ExplorerItem>();
            }
            return m_children;
        }
        set
        {
            m_children = value;
        }
    }

    private bool m_isExpanded;
    public bool IsExpanded
    {
        get { return m_isExpanded; }
        set { Set(ref m_isExpanded, value); }
    }
}
