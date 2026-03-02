using Ember.Scripting;
using Microsoft.EntityFrameworkCore;

namespace EFModeling.EntityProperties.FluentAPI.Required;

public class MyContext : DbContext
{
    public MyContext(DbContextOptions<MyContext> options) : base(options)
    {
    }
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
            .Property(b => b.SourceCode)
            .IsRequired();
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
