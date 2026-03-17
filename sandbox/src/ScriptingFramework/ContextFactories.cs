using Ember.Scripting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace GeneratorContextNoInherVaccineV5
{
    public class ContextFactory : Ember.Scripting.IContextFactory
    {
        public static GeneratorContextSF Create(ILabOrderInterfaceV4NoInheritence labOrder, IVaccineInterface vaccine)
        {
            return new GeneratorContext(labOrder, vaccine);
        }

        public static GeneratorContextSF Create(IServiceProvider serviceProvider)
        {
            var labOrder = serviceProvider.GetRequiredService<LabOrder>();
            var vaccine = serviceProvider.GetRequiredService<Vaccine>();

            return new GeneratorContext(labOrder, vaccine);
        }
    }
}
namespace GeneratorContextV4
{
    public class ContextFactory : Ember.Scripting.IContextFactory
    {
        public static GeneratorContextSF Create(ILabOrderInterfaceV3 labOrder, PatientInterface patient, ConsoleLoggerInterface logger, DataAccessInterface data)
        {
            return new GeneratorContext(labOrder, patient, logger, data);
        }

        public static GeneratorContextSF Create(IServiceProvider serviceProvider)
        {
            var labOrder = serviceProvider.GetRequiredService<LabOrder>();
            var patient = serviceProvider.GetRequiredService<Patient>();
            var logger = serviceProvider.GetRequiredService<ConsoleLogger>();
            var data = serviceProvider.GetRequiredService<DataAccess>();

            return new GeneratorContext(labOrder, patient, logger, data);
        }
    }
}
namespace GeneratorContextV3
{
    public class ContextFactory : Ember.Scripting.IContextFactory
    {
        public static GeneratorContextSF Create(ILabOrderInterfaceV2 labOrder, PatientInterface patient, ConsoleLoggerInterface logger, DataAccessInterface data)
        {
            return new GeneratorContext(labOrder, patient, logger, data);
        }

        public static GeneratorContextSF Create(IServiceProvider serviceProvider)
        {
            var labOrder = serviceProvider.GetRequiredService<LabOrder>();
            var patient = serviceProvider.GetRequiredService<Patient>();
            var logger = serviceProvider.GetRequiredService<ConsoleLogger>();
            var data = serviceProvider.GetRequiredService<DataAccess>();

            return new GeneratorContext(labOrder, patient, logger, data);
        }
    }
}
namespace RWContextV2
{
    public class ContextFactory : Ember.Scripting.IContextFactory
    {
        public static GeneratorContextSF Create(ILabOrderRWInterface labOrder, PatientInterface patient, ConsoleLoggerInterface logger, DataAccessInterface data)
        {
            return new GeneratorContext(labOrder, patient, logger, data);
        }

        public static GeneratorContextSF Create(IServiceProvider serviceProvider)
        {
            var labOrder = serviceProvider.GetRequiredService<LabOrder>();
            var patient = serviceProvider.GetRequiredService<Patient>();
            var logger = serviceProvider.GetRequiredService<ConsoleLogger>();
            var data = serviceProvider.GetRequiredService<DataAccess>();

            return new GeneratorContext(labOrder, patient, logger, data);
        }
    }
}
namespace ReadOnlyContextV1
{
    public class ContextFactory : Ember.Scripting.IContextFactory
    {
        public static GeneratorContextSF Create(ILabOrderInterface labOrder, PatientInterface patient, ConsoleLoggerInterface logger, DataAccessInterface data)
        {
            return new GeneratorContext(labOrder, patient, logger, data);
        }

        public static GeneratorContextSF Create(IServiceProvider serviceProvider)
        {
            var labOrder = serviceProvider.GetRequiredService<LabOrder>();
            var patient = serviceProvider.GetRequiredService<Patient>();
            var logger = serviceProvider.GetRequiredService<ConsoleLogger>();
            var data = serviceProvider.GetRequiredService<DataAccess>();

            return new GeneratorContext(labOrder, patient, logger, data);
        }
    }
}
namespace Sandbox
{
    internal static class ScriptingServiceCollectionExtensions
    {
        internal static IServiceCollection AddSandboxData
        (this IServiceCollection services, LabOrder labOrder, Patient patient, ConsoleLogger logger, DataAccess testDataAccess, Vaccine vaccine)
        {
            services.AddSingleton(labOrder);
            services.AddSingleton(patient);
            services.AddSingleton(logger);
            services.AddSingleton(testDataAccess);
            services.AddSingleton(vaccine);

            return services;
        }
    }
}