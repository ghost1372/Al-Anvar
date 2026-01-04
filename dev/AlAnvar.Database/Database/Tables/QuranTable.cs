using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace AlAnvar.Database.Tables;

[Table("QuranPlain")]
public partial class QuranTable
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public int SuraId { get; set; }
    public int AyaId { get; set; }
    public int JuzId { get; set; }
    public int HizbId { get; set; }
    public string Aya { get; set; }
    public string AudioFileName { get; set; }
}

[Table("QuranClean")]
public partial class QuranCleanTable
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public int SuraId { get; set; }
    public int AyaId { get; set; }
    public int JuzId { get; set; }
    public int HizbId { get; set; }
    public string Aya { get; set; }
    public string AudioFileName { get; set; }
}
