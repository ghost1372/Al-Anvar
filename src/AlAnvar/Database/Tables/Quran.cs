﻿namespace AlAnvar.Database.Tables;

[Table("QuranPlain")]
public class Quran
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    public int SurahId { get; set; }
    public int AyahNumber { get; set; }
    public string AyahText { get; set; }
    public int Juz { get; set; }
    public int Hizb { get; set; }
    public string Audio { get; set; }
}
