using Ember.Scripting;
using Microsoft.EntityFrameworkCore;

namespace EFModeling.EntityProperties.FluentAPI.Required;

public class MyContext : DbContext
{
    // 1. Add this constructor to accept configuration from Ember's Dependency Injection
    public MyContext(DbContextOptions<MyContext> options) : base(options)
    {
    }

    // Keep the parameterless constructor for local EF Core tooling/migrations
    public MyContext()
    {
    }

    public DbSet<CustomerScript> CustomerScripts { get; set; }
    public DbSet<ScriptCompiledCache> ScriptCompiledCaches { get; set; }
    public DbSet<EmberInstance> EmberInstances { get; set; }

    #region Required
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CustomerScript>()   //enforces source code not null
            .Property(b => b.SourceCode)        //todo source code is required rest can be extrapolated?    //this whole thing might be unnessesary
            .IsRequired();
        // modelBuilder.Entity<ScriptCompiledCache>()
        //   .Property(b => b.ScriptId)
        //   .IsRequired();
    }
    #endregion
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (optionsBuilder.IsConfigured == false)
        {
            optionsBuilder.UseNpgsql(
            @"Host=localhost;Port=5432;Database=script_registry;Username=admin;Password=your_secure_password");
        }

    }
}
