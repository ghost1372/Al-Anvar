namespace AlAnvar.Models;

public partial class SuraNode
{
    public int SuraId { get; set; }
    public string SuraName { get; set; }

    public List<AyaNode> Ayas { get; set; } = new();
}
