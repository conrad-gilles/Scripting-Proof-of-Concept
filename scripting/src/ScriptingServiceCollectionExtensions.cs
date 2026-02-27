using Microsoft.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Ember.Scripting;

//Ai Generated
public static class ScriptingServiceCollectionExtensions
{
    public static IServiceCollection AddEmberScripting(this IServiceCollection services, MetadataReference[] references)
    {
        // 1. Register the references so they can be injected into constructors
        services.AddSingleton(references);

        // 2. Register the internal classes
        // DI can instantiate internal classes as long as their constructors are marked 'public'
        services.AddTransient<ScriptCompiler>();
        services.AddTransient<ScriptExecutor>();

        // 3. Register DbHelper using a factory so we can keep its constructor internal
        services.AddTransient<DbHelper>(sp => new DbHelper(
            sp.GetRequiredService<ScriptCompiler>(),
            sp.GetRequiredService<MetadataReference[]>(),
            sp.GetRequiredService<ILogger<DbHelper>>()
        ));

        // 4. Register the Facade using a factory to hide the internal parameters
        // Step A: Register the concrete class using the factory so it can use the internal constructor
        services.AddTransient<ScriptManagerFacade>(sp => new ScriptManagerFacade(
            sp.GetRequiredService<DbHelper>(),
            sp.GetRequiredService<ScriptCompiler>(),
            sp.GetRequiredService<ScriptExecutor>(),
            sp.GetRequiredService<MetadataReference[]>(),
            sp.GetRequiredService<ILogger<ScriptManagerFacade>>()
        ));

        // Step B: Point both interfaces to the exact same concrete registration above
        services.AddTransient<IScriptManager>(sp => sp.GetRequiredService<ScriptManagerFacade>());
        services.AddTransient<IScriptManagerExtended>(sp => sp.GetRequiredService<ScriptManagerFacade>());

        return services;
    }
}
