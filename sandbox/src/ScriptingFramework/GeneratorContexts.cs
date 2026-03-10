using Microsoft.Extensions.Logging;
using Ember.Scripting;
using IGeneratorReadOnlyContextV1;

//todo version the Interfaces IGenerator bla bla

namespace IGeneratorReadOnlyContextV1
{
    public interface IGeneratorContext : IGeneratorBaseInterface
    {
        new static sealed int IVersion => 1;
        ILabOrderInterface LabOrder { get; }      // Read current order data
        PatientInterface Patient { get; }     // Read patient data
        ConsoleLoggerInterface Logger { get; }
        DataAccessInterface Data { get; }
    }
}

namespace IGeneratorContext_V2
{
    public interface IGeneratorContext : IGeneratorReadOnlyContextV1.IGeneratorContext
    {
        new static sealed int IVersion => 2;
        new ILabOrderRWInterface LabOrder { get; }

        // List<Patient> Patients;

    }
}

namespace IGeneratorContext_V3
{
    public interface IGeneratorContext : IGeneratorContext_V2.IGeneratorContext
    {
        new static sealed int IVersion => 3;
        new ILabOrderInterfaceV2 LabOrder { get; }
    }
}


namespace IGeneratorContext_V4
{
    public interface IGeneratorContext : IGeneratorContext_V3.IGeneratorContext
    {
        new static sealed int IVersion => 4;
        new ILabOrderInterfaceV3 LabOrder { get; }
    }
}
namespace IGeneratorContextNoInheritance_V5
{
    public interface IGeneratorContext : IGeneratorBaseInterface
    {
        new static sealed int IVersion => 5;
        ILabOrderInterfaceV4NoInheritence LabOrder { get; }
        IVaccineInterface Vaccine { get; }
    }
}



/// <summary>
/// Classes of context come here
/// </summary>
namespace ReadOnlyContextV1
{
    public class GeneratorContext : Ember.Scripting.GeneratorContext, IGeneratorReadOnlyContextV1.IGeneratorContext
    {
        public override int Version => 1;
        public int IVersion => Version;

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
        ILabOrderInterface IGeneratorReadOnlyContextV1.IGeneratorContext.LabOrder => labOrder;
        PatientInterface IGeneratorReadOnlyContextV1.IGeneratorContext.Patient => patient;
        ConsoleLoggerInterface IGeneratorReadOnlyContextV1.IGeneratorContext.Logger => logger;
        DataAccessInterface IGeneratorReadOnlyContextV1.IGeneratorContext.Data => data;
    }
}

namespace RWContextV2
{
    public class GeneratorContext : Ember.Scripting.GeneratorContext, IGeneratorContext_V2.IGeneratorContext
    {
        public override int Version => 2;
        public int IVersion => Version;

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

        ILabOrderInterface IGeneratorReadOnlyContextV1.IGeneratorContext.LabOrder => LabOrder;
    }
}

namespace GeneratorContextV3
{
    public class GeneratorContext : RWContextV2.GeneratorContext, IGeneratorContext_V3.IGeneratorContext
    {
        public override int Version => 3;
        // public int IVersion => Version;

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
        ILabOrderInterfaceV2 IGeneratorContext_V3.IGeneratorContext.LabOrder => labOrderV2;

    }
}

namespace GeneratorContextV4
{
    public class GeneratorContext : GeneratorContextV3.GeneratorContext, IGeneratorContext_V4.IGeneratorContext  //todo implement Adapter pattern
    {
        public override int Version => 4;
        // public int IVersion => Version;

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
        ILabOrderInterfaceV3 IGeneratorContext_V4.IGeneratorContext.LabOrder => labOrderV3;

    }
}

namespace GeneratorContextNoInherVaccineV5
{
    public class GeneratorContext : Ember.Scripting.GeneratorContext, IGeneratorContextNoInheritance_V5.IGeneratorContext   //into diffrent namespaces blocks later folders
    {
        public override int Version => 5;
        public int IVersion => Version;

        ILabOrderInterfaceV4NoInheritence LabOrder;
        IVaccineInterface Vaccine;
        public GeneratorContext(ILabOrderInterfaceV4NoInheritence labOrder, IVaccineInterface vaccine)
        {
            LabOrder = labOrder;
            Vaccine = vaccine;
        }

        ILabOrderInterfaceV4NoInheritence IGeneratorContextNoInheritance_V5.IGeneratorContext.LabOrder => LabOrder;

        IVaccineInterface IGeneratorContextNoInheritance_V5.IGeneratorContext.Vaccine => Vaccine;
    }
}


//context not always backwards comp, make sure right context passed factory
