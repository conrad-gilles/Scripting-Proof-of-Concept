using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Ember.Scripting;

//Ai Generated
public static class ScriptingServiceCollectionExtensions
{
    public static IServiceCollection AddEmberScripting(this IServiceCollection services, List<MetadataReference> references, int maxSupportedApiVersion, List<Type> recentTypes)
    {
        // 1. Register the references so they can be injected into constructors
        services.AddSingleton(references);
        services.AddSingleton(recentTypes);

        // 2. Register the internal classes
        // DI can instantiate internal classes as long as their constructors are marked 'public'
        services.AddTransient<ScriptCompiler>();
        services.AddTransient<ScriptExecutor>();

        // 3. Register DbHelper using a factory so we can keep its constructor internal
        services.AddTransient(sp => new ScriptRepository(
            sp.GetRequiredService<ScriptCompiler>(),
            sp.GetRequiredService<List<MetadataReference>>(),
            sp.GetRequiredService<ILogger<ScriptRepository>>(),
            maxSupportedApiVersion,
            sp.GetRequiredService<IDbContextFactory<ScriptDbContext>>(),
            sp.GetRequiredService<IUserSession>()
        ));

        // 4. Register the Facade using a factory to hide the internal parameters
        // Step A: Register the concrete class using the factory so it can use the internal constructor
        services.AddTransient(sp => new ScriptManagerBase(
            sp.GetRequiredService<ScriptRepository>(),
            sp.GetRequiredService<ScriptCompiler>(),
            sp.GetRequiredService<ScriptExecutor>(),
            sp.GetRequiredService<ILogger<ScriptManagerBase>>(),
            sp.GetRequiredService<IUserSession>()
        ));

        services.AddTransient(sp => new ScriptManager(    //maybe in the future swittch to singleton
        sp.GetRequiredService<IScriptManagerBaseExtended>()
        ));
        services.AddSingleton<IScriptManager>(sp => sp.GetRequiredService<ScriptManager>());

        // Step B: Point both interfaces to the exact same concrete registration above
        services.AddTransient<IScriptManagerBase>(sp => sp.GetRequiredService<ScriptManagerBase>());
        services.AddTransient<IScriptManagerBaseExtended>(sp => sp.GetRequiredService<ScriptManagerBase>());

        return services;
    }
}
