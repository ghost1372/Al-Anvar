namespace AlAnvar.Models;

public partial class AyaNode
{
    public int SuraId { get; set; }
    public int AyaId { get; set; }
    public string AyaText { get; set; }
    public string NoteCount => Notes.Count.ToString();
    public List<NoteNode> Notes { get; set; } = new();
}
