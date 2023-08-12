namespace AlAnvar.Database;

public class AlAnvarDBContext : DbContext
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var dbFile = @$"{AppContext.BaseDirectory}\Assets\DataBase\Al-Anvar.db";
        optionsBuilder.UseSqlite($"Data Source={dbFile}");
    }

    public DbSet<ChapterProperty> Chapters { get; set; }
    public DbSet<Quran> Qurans { get; set; }
    public DbSet<QuranSearch> QuranSearches { get; set; }
    public DbSet<QuranTranslation> Translations { get; set; }
    public DbSet<QuranAudio> Audios { get; set; }
    public DbSet<TranslationsText> TranslationsText { get; set; }
}
