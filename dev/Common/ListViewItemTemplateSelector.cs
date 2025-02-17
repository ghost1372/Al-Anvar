namespace AlAnvar.Common;
public class ListViewItemTemplateSelector : DataTemplateSelector
{
    public DataTemplate QuranTemplate { get; set; }
    public DataTemplate TranslationTemplate { get; set; }

    protected override DataTemplate SelectTemplateCore(object item)
    {
        if (item == null) return null;
        var search = item as QuranSearch2;
        var searchOptionIndex = 0;
        if (MainWindow.Instance != null)
        {
            searchOptionIndex = MainWindow.Instance.GetQuranSearchOptionIndex();
        }

        if (searchOptionIndex == 0 && search.IsTranslation)
        {
            return TranslationTemplate;
        }
        else if (searchOptionIndex == 0 && !search.IsTranslation)
        {
            return QuranTemplate;
        }
        else
        {
            return QuranTemplate;
        }
    }
}
