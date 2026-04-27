using Ember.Scripting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
// using 

//The active ContextFactory
namespace ContextFactoryNameSpace
{
    public class ContextFactory : IGeneratorContextFactory, ISomeOtherContextFactory
    {
        public RecentIGeneratorContext CreateGeneratorContext(ILabOrderInterfaceV4NoInheritence labOrder, IVaccineInterface vaccine)
        {
            return new RecentGeneratorContext(labOrder, vaccine);
        }

        public RecentIGeneratorContext CreateOtherFactory(ILabOrderInterfaceV4NoInheritence labOrder, IVaccineInterface vaccine)
        {
            throw new NotImplementedException();
        }
    }
    public interface IGeneratorContextFactory
    {
        public RecentIGeneratorContext CreateGeneratorContext(ILabOrderInterfaceV4NoInheritence labOrder, IVaccineInterface vaccine);
    }
    public interface ISomeOtherContextFactory
    {
        public RecentIGeneratorContext CreateOtherFactory(ILabOrderInterfaceV4NoInheritence labOrder, IVaccineInterface vaccine);
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

            // // Register the factory
            // services.AddTransient<RecentGeneratorContextFactory.IGeneratorContextFactory, RecentGeneratorContextFactory.ContextFactory>();

            // Register the concrete factory
            services.AddTransient<ContextFactoryNameSpace.ContextFactory>();

            // Forward interfaces to the concrete implementation
            services.AddTransient<ContextFactoryNameSpace.IGeneratorContextFactory>(sp => sp.GetRequiredService<ContextFactoryNameSpace.ContextFactory>());
            services.AddTransient<ContextFactoryNameSpace.ISomeOtherContextFactory>(sp => sp.GetRequiredService<ContextFactoryNameSpace.ContextFactory>());

            return services;
        }
    }
}