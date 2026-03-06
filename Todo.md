# Todo
- add a constructor to the customerScript class maybe add 2, 1 empty one so stuff dont break
- store old source code versions in dictionary in db  map it to active api versions should be fine but maybe test?
- pass data in context so when you execute script(id,context) it gives data to the script
- this data needs to be standardized maybe using something like fluentvalidation idk
- fix basic val still doesnt check if implements correct class
- give user possibility to save not working script
- in ui when compile all scripts with 1 corrupt one more than 1 dont cpompile because it aborts the process
- make a function in random methods to init injection faster
- make a more efficient IsDuplicateScript function in dbhelper

# Converting to a class library:
### In sandbox.csproj make sure to uncomment:
`<OutputType>Library</OutputType>`

and comment the line above.

### Remove MainProgram.cs
move it to seperate test/console project that references the library.


3. Provide an Extension Method for Ember
To make your class library truly professional and easy for Ember to consume, you should provide an IServiceCollection extension method. This allows the Ember developers to register your entire scripting engine with a single line of code in their Program.cs or Startup.cs.

Create a new file called EmberScriptingExtensions.cs in your class library:

csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EFModeling.EntityProperties.FluentAPI.Required;

public static class EmberScriptingExtensions
{
    // Ember will call this method on startup
    public static IServiceCollection AddEmberScripting(this IServiceCollection services, string databaseConnectionString)
    {
        // 1. Register the Database Context
        services.AddDbContext<MyContext>(options =>
            options.UseNpgsql(databaseConnectionString));

        // 2. Register your library's core classes
        services.AddScoped<DbHelper>();
        services.AddScoped<ScriptCompiler>();
        services.AddScoped<ScriptManagerFacade>();

        // (If ScriptExecutor is instantiated per script, you might register it as Transient)
        services.AddTransient<ScriptExecutor>();

        return services;
    }
}
How Ember Will Use Your Library
Once you implement these changes, your boss or the Ember team will simply reference your .dll and add this exact code to their main application startup:

csharp
// Inside Ember's Program.cs:
builder.Services.AddEmberScripting(
    builder.Configuration.GetConnectionString("EmberScriptingDB")
);
When an Ember controller or background service needs to run a script, they will simply ask for your ScriptManagerFacade in their constructor, and the DI container will automatically build the MyContext, pass it to DbHelper, pass DbHelper to ScriptManagerFacade, and deliver the fully initialized object.
