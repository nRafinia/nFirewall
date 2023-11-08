using System.Reflection;
using Microsoft.EntityFrameworkCore;
using nFirewall.Domain.Entities;

namespace nFirewall.Persistence;

public class ApplicationDbContext : DbContext
{
    public DbSet<BannedAddress> BannedAddresses { get; set; }
    
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        Database.Migrate();
        
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        base.OnModelCreating(modelBuilder);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.LogTo(Console.WriteLine);
    }
}