// using EFModeling.EntityProperties.FluentAPI.Required;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Ember.Scripting.Compilation;

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
        services.AddTransient<ScriptRepository>(sp => new ScriptRepository(
            sp.GetRequiredService<ScriptCompiler>(),
            sp.GetRequiredService<List<MetadataReference>>(),
            sp.GetRequiredService<ILogger<ScriptRepository>>(),
            maxSupportedApiVersion,
            sp.GetRequiredService<IDbContextFactory<ScriptDbContext>>(),
            sp.GetRequiredService<IUserSession>()
        ));

        // 4. Register the Facade using a factory to hide the internal parameters
        // Step A: Register the concrete class using the factory so it can use the internal constructor
        services.AddTransient<ScriptManagerFacade>(sp => new ScriptManagerFacade(
            sp.GetRequiredService<ScriptRepository>(),
            sp.GetRequiredService<ScriptCompiler>(),
            sp.GetRequiredService<ScriptExecutor>(),
            sp.GetRequiredService<List<MetadataReference>>(),
            sp.GetRequiredService<ILogger<ScriptManagerFacade>>(),
            maxSupportedApiVersion,
             sp.GetRequiredService<IUserSession>()
        ));

        // Step B: Point both interfaces to the exact same concrete registration above
        services.AddTransient<IScriptManager>(sp => sp.GetRequiredService<ScriptManagerFacade>());
        services.AddTransient<IScriptManagerExtended>(sp => sp.GetRequiredService<ScriptManagerFacade>());
        services.AddTransient<IScriptManagerDeleteAfter>(sp => sp.GetRequiredService<ScriptManagerFacade>());

        return services;
    }
}
