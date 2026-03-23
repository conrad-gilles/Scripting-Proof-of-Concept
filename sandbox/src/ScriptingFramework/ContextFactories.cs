using Ember.Scripting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;


namespace GeneratorContextNoInherVaccineV5
{
    public class ContextFactory : IGeneratorContextFactory
    {
        public ActiveGeneratorContext Create(ILabOrderInterfaceV4NoInheritence labOrder, IVaccineInterface vaccine)
        {
            return new ActiveGeneratorContext(labOrder, vaccine);
        }
    }

    public interface IGeneratorContextFactory
    {
        public ActiveGeneratorContext Create(ILabOrderInterfaceV4NoInheritence labOrder, IVaccineInterface vaccine);
    }
}
namespace GeneratorContextV4
{
    public class ContextFactory(IConsoleLoggerInterface logger, IDataAccessInterface data) : IGeneratorContextFactory
    {
        public GeneratorContextV4.GeneratorContext Create(ILabOrderInterfaceV3 labOrder, IPatientInterface patient, IVaccineInterface vaccine)
        {
            return new GeneratorContextV4.GeneratorContext(labOrder, patient, logger, data);
        }
    }

    public interface IGeneratorContextFactory
    {
        public GeneratorContextV4.GeneratorContext Create(ILabOrderInterfaceV3 labOrder, IPatientInterface patient, IVaccineInterface vaccine);
    }
}
namespace GeneratorContextV3
{
    public class ContextFactory(IConsoleLoggerInterface logger, IDataAccessInterface data) : IGeneratorContextFactory
    {
        public GeneratorContextV3.GeneratorContext Create(ILabOrderInterfaceV2 labOrder, IPatientInterface patient)
        {
            return new GeneratorContextV3.GeneratorContext(labOrder, patient, logger, data);
        }
    }

    public interface IGeneratorContextFactory
    {
        public GeneratorContextV3.GeneratorContext Create(ILabOrderInterfaceV2 labOrder, IPatientInterface patient);
    }
}
namespace RWContextV2
{
    public class ContextFactory(IConsoleLoggerInterface logger, IDataAccessInterface data) : IGeneratorContextFactory
    {
        public RWContextV2.GeneratorContext Create(ILabOrderRWInterface labOrder, IPatientInterface patient)
        {
            return new RWContextV2.GeneratorContext(labOrder, patient, logger, data);
        }
    }

    public interface IGeneratorContextFactory
    {
        public RWContextV2.GeneratorContext Create(ILabOrderRWInterface labOrder, IPatientInterface patient);
    }
}
namespace ReadOnlyContextV1
{
    public class ContextFactory(IConsoleLoggerInterface logger, IDataAccessInterface data) : IGeneratorContextFactory
    {
        public ReadOnlyContextV1.GeneratorContext Create(ILabOrderInterface labOrder, IPatientInterface patient)
        {
            return new ReadOnlyContextV1.GeneratorContext(labOrder, patient, logger, data);
        }
    }

    public interface IGeneratorContextFactory
    {
        public ReadOnlyContextV1.GeneratorContext Create(ILabOrderInterface labOrder, IPatientInterface patient);
    }
}

namespace Sandbox
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
            services.AddTransient<ActiveContextFactory.IGeneratorContextFactory, ActiveContextFactory.ContextFactory>();
            return services;
        }
    }
}