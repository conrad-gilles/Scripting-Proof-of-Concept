using Ember.Scripting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
// using 

//The active ContextFactory
namespace GeneratorContextNoInherVaccineV5
{
    public class ContextFactory : IGeneratorContextFactory
    {
        public RecentGeneratorContext Create(ILabOrderInterfaceV4NoInheritence labOrder, IVaccineInterface vaccine)
        {
            return new RecentGeneratorContext(labOrder, vaccine);
        }
    }

    public interface IGeneratorContextFactory
    {
        public RecentGeneratorContext Create(ILabOrderInterfaceV4NoInheritence labOrder, IVaccineInterface vaccine);
    }
}

namespace Ember.Simulation
{
    internal static class SandboxServiceCollectionExtensions
    {
        internal static IServiceCollection AddSandboxServices
               (this IServiceCollection services, ConsoleLogger logger, DataAccess testDataAccess)
        {
            // Register services
            services.AddSingleton(logger);
            services.AddSingleton(testDataAccess);

            // Register the factory
            services.AddTransient<RecentContextFactory.IGeneratorContextFactory, RecentContextFactory.ContextFactory>();
            return services;
        }
    }
}