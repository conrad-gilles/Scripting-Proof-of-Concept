using Ember.Scripting;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;

namespace EFModeling.EntityProperties.FluentAPI.Required;

public class ScriptDbContext : DbContext //, IDataProtectionKeyContext
{
    public ScriptDbContext(DbContextOptions<ScriptDbContext> options) : base(options)
    {
    }
    public ScriptDbContext()
    {
    }

    // public DbSet<DataProtectionKey> DataProtectionKeys { get; set; } = null!;   //todo this is here for blazor app to prevent loss of performance doesnt work yet app always crashes on Render.com

    public DbSet<CustomerScript> CustomerScripts { get; set; }
    public DbSet<CompiledScript> ScriptCompiledCaches { get; set; }
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
            connectionString = Environment.GetEnvironmentVariable("DATABASE_URL")!;

            bool useContainer = true;
            if (useContainer)
            {
                if (string.IsNullOrEmpty(connectionString)) //using PostgreSQL container
                {
                    connectionString = "Host=localhost;Port=5432;Database=script_registry;Username=admin;Password=your_secure_password";
                }
            }
            else
            {
                if (string.IsNullOrEmpty(connectionString)) //using Neon.tech db, in a real application ofc you would never hardcode this but pass as environment variable like above
                {
                    connectionString = "Host=ep-lingering-boat-agx7ywj8.c-2.eu-central-1.aws.neon.tech;Database=scriptsDB;Username=neondb_owner;Password=npg_XcDBEPxH7m9o;SSL Mode=Require;Trust Server Certificate=true;";
                }
            }

            optionsBuilder.UseNpgsql(connectionString);
            // optionsBuilder.UseNpgsql(connectionString, b => b.MigrationsAssembly("BlazorUI"));  //todo might have to delete for local testing but necessary to store the instructions on how to generate the tables

        }

    }
}
