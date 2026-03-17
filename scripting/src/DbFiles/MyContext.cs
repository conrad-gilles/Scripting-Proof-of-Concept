using Ember.Scripting;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;

namespace EFModeling.EntityProperties.FluentAPI.Required;

public class MyContext : DbContext, IDataProtectionKeyContext
{
    public MyContext(DbContextOptions<MyContext> options) : base(options)
    {
    }
    public MyContext()
    {
    }

    public DbSet<DataProtectionKey> DataProtectionKeys { get; set; } = null!;
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
            // 1. Try to get the connection string from Koyeb's environment variables
            var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL");

            // 2. If it is null or empty (meaning you are running locally), fall back to localhost
            if (string.IsNullOrEmpty(connectionString))
            {
                connectionString = "Host=localhost;Port=5432;Database=script_registry;Username=admin;Password=your_secure_password";
            }

            // optionsBuilder.UseNpgsql(connectionString);
            optionsBuilder.UseNpgsql(connectionString, b => b.MigrationsAssembly("BlazorUI"));

        }

    }
}
