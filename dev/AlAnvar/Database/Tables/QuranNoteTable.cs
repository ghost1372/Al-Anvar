using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlAnvar.Database.Tables;

[Table("Notes")]
public partial class QuranNoteTable
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    public int SuraId { get; set; }
    public int AyaId { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string CreatedAt { get; set; }
    public string UpdatedAt { get; set; }
}
