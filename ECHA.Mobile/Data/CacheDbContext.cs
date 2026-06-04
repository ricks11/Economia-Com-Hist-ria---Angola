using Microsoft.EntityFrameworkCore;
using ECHA.Mobile.Models;

namespace ECHA.Mobile.Data;

public class CacheDbContext : DbContext
{
    public DbSet<OfflineContent> OfflineContents => Set<OfflineContent>();
    public DbSet<QuizAttempt> QuizAttempts => Set<QuizAttempt>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        string dbPath = Path.Combine(FileSystem.AppDataDirectory, "echa_cache.db");
        optionsBuilder.UseSqlite($"Filename={dbPath}");
    }
}

public class OfflineContent
{
    public Guid Id { get; set; }
    public string JsonData { get; set; } = string.Empty;
}

public class QuizAttempt
{
    public Guid Id { get; set; }
    public Guid QuizId { get; set; }
    public string Data { get; set; } = string.Empty; // JSON attempt
    public DateTime Timestamp { get; set; }
    public bool IsSynced { get; set; }
}
