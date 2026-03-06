using Microsoft.Extensions.Logging;
using Ember.Scripting;
using IGeneratorReadOnlyContext;

namespace IGeneratorReadOnlyContext
{
    public interface IGeneratorContext : IGeneratorBaseInterface
    {
        ILabOrderInterface LabOrder { get; }      // Read current order data
        PatientInterface Patient { get; }     // Read patient data
        ConsoleLoggerInterface Logger { get; }
        DataAccessInterface Data { get; }
    }
}

namespace IGeneratorContext_V1
{
    public interface IGeneratorContext : IGeneratorReadOnlyContext.IGeneratorContext
    {
        new ILabOrderRWInterface LabOrder { get; }

        // List<Patient> Patients;

    }
}

namespace IGeneratorContext_V2
{
    public interface IGeneratorContext : IGeneratorContext_V1.IGeneratorContext
    {
        new ILabOrderInterfaceV2 LabOrder { get; }
    }
}


namespace IGeneratorContext_V3
{
    public interface IGeneratorContext : IGeneratorContext_V2.IGeneratorContext
    {
        new ILabOrderInterfaceV3 LabOrder { get; }
    }
}
namespace IGeneratorContextNoInheritance_V4
{
    public interface IGeneratorContext : IGeneratorBaseInterface
    {
        ILabOrderInterfaceV4NoInheritence LabOrder { get; }
        IVaccineInterface Vaccine { get; }
    }
}



/// <summary>
/// Classes of context come here
/// </summary>
namespace ReadOnlyContext
{
    public class GeneratorContext : Ember.Scripting.GeneratorContext, IGeneratorReadOnlyContext.IGeneratorContext
    {
        public ILabOrderInterface labOrder;
        public PatientInterface patient;

        public ConsoleLoggerInterface logger;
        public DataAccessInterface data;


        public GeneratorContext(ILabOrderInterface pLabOrder, PatientInterface pPatient, ConsoleLoggerInterface plogger, DataAccessInterface pdata)
        {
            labOrder = pLabOrder;
            patient = pPatient;
            logger = plogger;
            data = pdata;

        }
        ILabOrderInterface IGeneratorReadOnlyContext.IGeneratorContext.LabOrder => labOrder;
        PatientInterface IGeneratorReadOnlyContext.IGeneratorContext.Patient => patient;
        ConsoleLoggerInterface IGeneratorReadOnlyContext.IGeneratorContext.Logger => logger;
        DataAccessInterface IGeneratorReadOnlyContext.IGeneratorContext.Data => data;
    }
}

namespace RWContext
{
    public class GeneratorContext : Ember.Scripting.GeneratorContext, IGeneratorContext_V1.IGeneratorContext
    {
        public ILabOrderRWInterface labOrder;
        public PatientInterface patient;

        public ConsoleLoggerInterface logger;
        public DataAccessInterface data;


        public GeneratorContext(ILabOrderRWInterface pLabOrder, PatientInterface pPatient, ConsoleLoggerInterface plogger, DataAccessInterface pdata)
        {
            labOrder = pLabOrder;
            patient = pPatient;
            logger = plogger;
            data = pdata;

        }

        public ILabOrderRWInterface LabOrder => labOrder;

        public PatientInterface Patient => patient;

        public ConsoleLoggerInterface Logger => logger;

        public DataAccessInterface Data => data;

        ILabOrderInterface IGeneratorReadOnlyContext.IGeneratorContext.LabOrder => LabOrder;
    }
}

namespace GeneratorContextV2
{
    public class GeneratorContext : RWContext.GeneratorContext, IGeneratorContext_V2.IGeneratorContext
    {
        public ILabOrderInterfaceV2 labOrderV2;
        public new PatientInterface patient;

        public new ConsoleLoggerInterface logger;
        public new DataAccessInterface data;


        public GeneratorContext(ILabOrderInterfaceV2 pLabOrder, PatientInterface pPatient, ConsoleLoggerInterface plogger, DataAccessInterface pdata)
        : base(pLabOrder, pPatient, plogger, pdata)
        {
            labOrderV2 = pLabOrder;
            patient = pPatient;
            logger = plogger;
            data = pdata;

        }
        ILabOrderInterfaceV2 IGeneratorContext_V2.IGeneratorContext.LabOrder => labOrderV2;

    }
}

namespace GeneratorContextV3
{
    public class GeneratorContext : GeneratorContextV2.GeneratorContext, IGeneratorContext_V3.IGeneratorContext  //todo implement Adapter pattern
    {
        public ILabOrderInterfaceV3 labOrderV3;
        public new PatientInterface patient;

        public new ConsoleLoggerInterface logger;
        public new DataAccessInterface data;


        public GeneratorContext(ILabOrderInterfaceV3 pLabOrderV3, PatientInterface pPatient, ConsoleLoggerInterface plogger, DataAccessInterface pdata)
        : base(pLabOrderV3, pPatient, plogger, pdata)
        {
            labOrderV3 = pLabOrderV3;
            patient = pPatient;
            logger = plogger;
            data = pdata;

        }
        ILabOrderInterfaceV3 IGeneratorContext_V3.IGeneratorContext.LabOrder => labOrderV3;

    }
}

namespace GeneratorContextNoInherVaccine
{
    public class GeneratorContext : Ember.Scripting.GeneratorContext, IGeneratorContextNoInheritance_V4.IGeneratorContext   //into diffrent namespaces blocks later folders
    {
        ILabOrderInterfaceV4NoInheritence LabOrder;
        IVaccineInterface Vaccine;
        public GeneratorContext(ILabOrderInterfaceV4NoInheritence labOrder, IVaccineInterface vaccine)
        {
            LabOrder = labOrder;
            Vaccine = vaccine;
        }

        ILabOrderInterfaceV4NoInheritence IGeneratorContextNoInheritance_V4.IGeneratorContext.LabOrder => LabOrder;

        IVaccineInterface IGeneratorContextNoInheritance_V4.IGeneratorContext.Vaccine => Vaccine;
    }
}


//context not always backwards comp, make sure right context passed factory
