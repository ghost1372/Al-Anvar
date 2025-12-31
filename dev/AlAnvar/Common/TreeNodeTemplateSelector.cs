using AlAnvar.Models;

namespace AlAnvar.Common;

public partial class TreeNodeTemplateSelector : DataTemplateSelector
{
    public DataTemplate SuraTemplate { get; set; }
    public DataTemplate AyaTemplate { get; set; }
    public DataTemplate NoteTemplate { get; set; }

    protected override DataTemplate SelectTemplateCore(object item)
    {
        return item switch
        {
            SuraNode => SuraTemplate,
            AyaNode => AyaTemplate,
            NoteNode => NoteTemplate,
            _ => base.SelectTemplateCore(item)
        };
    }
}
