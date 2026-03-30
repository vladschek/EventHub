using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace EventHub.Infrastructure.Data;

public sealed class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        var cs = Environment.GetEnvironmentVariable("EVENTHUB_SQL")
            ?? "Server=(localdb)\\mssqllocaldb;Database=EventHub;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true";
        optionsBuilder.UseSqlServer(cs);
        return new AppDbContext(optionsBuilder.Options);
    }
}
