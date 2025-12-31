namespace AlAnvar.Models;

public partial class FontOption
{
    public string FontKey { get; set; }
    public string FontDisplayName { get; set; }

    public FontOption(string key, string name)
    {
        this.FontKey = key;
        this.FontDisplayName = name;
    }

    public override string ToString()
    {
        return FontDisplayName;
    }
}
