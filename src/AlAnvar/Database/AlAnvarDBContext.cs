namespace AlAnvar.Database;

public class AlAnvarDBContext : DbContext
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        //Todo: db path will be change
        optionsBuilder.UseSqlite(@"Data Source=D:\Programming\Github\Al-Anvar.db");
    }

    public DbSet<ChapterProperty> Chapters { get; set; }
    public DbSet<Quran> Qurans { get; set; }
    public DbSet<QuranSearch> QuranSearches { get; set; }
    public DbSet<QuranTranslation> Translations { get; set; }
    public DbSet<QuranAudio> Audios { get; set; }
    public DbSet<TafsirName> TafsirNames { get; set; }
    public DbSet<Tafsir> Tafsirs { get; set; }
}
