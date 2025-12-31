namespace AlAnvar.Models;

public partial class NoteNode
{
    public int NoteId { get; set; }
    public int SuraId { get; set; }
    public int AyaId { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string CreatedAt { get; set; }
    public string UpdatedAt { get; set; }
}
