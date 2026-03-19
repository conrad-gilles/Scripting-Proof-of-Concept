using BlazorUI.Components;
using Ember.Scripting;
using BlazorUI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.DataProtection;
// using sandbox; // only if RandomMethods is in the sandbox project/namespace


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Register Ember scripting services (DI)
var references = EmberMethods.GetReferences();
int version = EmberMethods.GetEmberApiVersion();
// string connectionString = "Host=localhost;Port=5432;Database=script_registry;Username=admin;Password=your_secure_password";

builder.Services.AddDbContextFactory<EFModeling.EntityProperties.FluentAPI.Required.MyContext>();

builder.Services.AddEmberScripting(references, version);
builder.Services.AddDbContext<EFModeling.EntityProperties.FluentAPI.Required.MyContext>();

// builder.Services.AddDataProtection().PersistKeysToDbContext<EFModeling.EntityProperties.FluentAPI.Required.MyContext>();


builder.Services.AddScoped<ConsoleService>();
var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<EFModeling.EntityProperties.FluentAPI.Required.MyContext>();

    // This will look for the Migrations folder and apply them to Neon automatically
    // db.Database.Migrate();
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();


app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
