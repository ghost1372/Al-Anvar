namespace AlAnvar.Models;
public partial class JsonTranslationFile
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Translator { get; set; }
    public string Language { get; set; }
    public string LastUpdate { get; set; }
    public List<JsonVerse> Verses { get; set; } = new();
}
