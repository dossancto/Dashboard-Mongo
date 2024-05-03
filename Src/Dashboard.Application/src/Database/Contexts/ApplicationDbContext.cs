using Dashboard.Application.Entities;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using MongoDB.EntityFrameworkCore.Extensions;

namespace Dashboard.Application.Database.Contexts;

public class ApplicationDbContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<MessageEvent> MessageEvents { get; init; } = default!;

    public static ApplicationDbContext Create(IMongoDatabase database) =>
        new(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseMongoDB(database.Client, database.DatabaseNamespace.DatabaseName)
            .Options);

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<MessageEvent>().ToCollection("movies");
    }

}
