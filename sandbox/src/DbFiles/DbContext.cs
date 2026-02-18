using Microsoft.EntityFrameworkCore;

namespace EFModeling.EntityProperties.FluentAPI.Required;

internal class MyContext : DbContext
{
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
        optionsBuilder.UseNpgsql(
            @"Host=localhost;Port=5432;Database=script_registry;Username=admin;Password=your_secure_password");
    }
}