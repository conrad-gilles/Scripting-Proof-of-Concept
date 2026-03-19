using Ember.Scripting;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;

namespace EFModeling.EntityProperties.FluentAPI.Required;

public class MyContext : DbContext //, IDataProtectionKeyContext
{
    public MyContext(DbContextOptions<MyContext> options) : base(options)
    {
    }
    public MyContext()
    {
    }

    // public DbSet<DataProtectionKey> DataProtectionKeys { get; set; } = null!;   //todo this is here for blazor app to prevent loss of performance doesnt work yet app always crashes on Render.com

    public DbSet<CustomerScript> CustomerScripts { get; set; }
    public DbSet<CompiledScripts> ScriptCompiledCaches { get; set; }
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
            //tries to get connection string from environment variable if not it falls back to the docker container
            string? connectionString = null;
            // connectionString = Environment.GetEnvironmentVariable("DATABASE_URL")!;

            if (string.IsNullOrEmpty(connectionString))
            {
                connectionString = "Host=localhost;Port=5432;Database=script_registry;Username=admin;Password=your_secure_password";
            }

            optionsBuilder.UseNpgsql(connectionString);
            // optionsBuilder.UseNpgsql(connectionString, b => b.MigrationsAssembly("BlazorUI"));  //todo might have to delete for local testing but necessary to store the instructions on how to generate the tables

        }

    }
}
