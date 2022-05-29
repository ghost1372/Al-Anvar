namespace AlAnvar.Common;
public class ExplorerItemTemplateSelector : DataTemplateSelector
{
    public DataTemplate FolderTemplate { get; set; }
    public DataTemplate FileTemplate { get; set; }

    protected override DataTemplate SelectTemplateCore(object item)
    {
        var explorerItem = (ExplorerItem) item;
        if (explorerItem.Type == ExplorerItem.ExplorerItemType.Folder) return FolderTemplate;

        return FileTemplate;
    }
}
