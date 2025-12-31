namespace AlAnvar.Models;
public partial class TranslationItem
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Translator { get; set; }
    public string Language { get; set; }
    public string Link { get; set; }
    public bool IsActive { get; set; }
    public override string ToString() => $"{Language} - {Translator}"; // what shows in ComboBox
}
