namespace AlAnvar.Common;
public class ExplorerItemTemplateSelector : DataTemplateSelector
{
    public DataTemplate FolderTemplate { get; set; }
    public DataTemplate FileTemplate { get; set; }
    public DataTemplate CheckMarkTemplate { get; set; }

    protected override DataTemplate SelectTemplateCore(object item)
    {
        var explorerItem = (ExplorerItem) item;
        switch (explorerItem.Type)
        {
            case ExplorerItem.ExplorerItemType.Folder:
                return FolderTemplate;
            case ExplorerItem.ExplorerItemType.File:
                return FileTemplate;
            case ExplorerItem.ExplorerItemType.CheckMark:
                return CheckMarkTemplate;
            default:
                return base.SelectTemplateCore(item);
        }
    }
}
