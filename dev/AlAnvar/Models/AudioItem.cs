namespace AlAnvar.Models;
public partial class AudioItem
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string PersianName { get; set; }
    public string DirName { get; set; }
    public string Url { get; set; }
    public bool IsActive { get; set; }
    public override string ToString() => $"{Name} - {PersianName}"; // what shows in ComboBox
}
