using EventHub.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace EventHub.Infrastructure.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Event> Events => Set<Event>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Event>(entity =>
        {
            entity.ToTable("Events");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UserId).HasMaxLength(256).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(2000).IsRequired();
            entity.Property(e => e.Type)
                .HasConversion<string>()
                .HasMaxLength(32)
                .IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();

            entity.HasIndex(e => e.CreatedAt).HasDatabaseName("IX_Events_CreatedAt");
            entity.HasIndex(e => e.UserId).HasDatabaseName("IX_Events_UserId");
            entity.HasIndex(e => e.Type).HasDatabaseName("IX_Events_Type");
        });
    }
}
