using System.Diagnostics;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
// using EFModeling.EntityProperties.FluentAPI.Required;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Ember.Scripting;
using System.Reflection;
using Serilog.Extensions.Logging;
using Microsoft.Extensions.Logging;
using Ember.Scripting.Compilation;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Sandbox;

try
{

    using (var db = new ScriptDbContext())
    {
        await db.Database.EnsureDeletedAsync();
        await db.Database.EnsureCreatedAsync();
    }

    var logger = new LoggerForScripting();
    Log.Logger = logger.SetUpAndGetSeriLogger();
    Log.Debug("First Log Test.");

    var services = new ServiceCollection();
    services.AddLogging(builder =>
    {
        builder.AddSerilog(dispose: true);
    });

    services.AddDbContextFactory<ScriptDbContext>();
    services.AddSingleton<IUserSession, SandBoxUserSession>();

    ScriptingServiceCollectionExtensions.AddEmberScripting(services, EmberMethods.GetReferences(), EmberMethods.GetEmberApiVersion(), RecentTypeHelper.GetRecentTypes());

    var provider = services.BuildServiceProvider();
    IScriptManagerExtended facade = provider.GetRequiredService<IScriptManagerExtended>();
}
finally
{
    await Log.CloseAndFlushAsync();
}

