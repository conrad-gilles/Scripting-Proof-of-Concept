using Microsoft.Extensions.Logging;
using Ember.Scripting;
using IGeneratorReadOnlyContextV1;

//todo version the Interfaces IGenerator bla bla

namespace IGeneratorReadOnlyContextV1
{
    [MetaDataIGeneratorIntrfc(version: 1)]
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
    [MetaDataIGeneratorIntrfc(version: 2)]
    public interface IGeneratorContext : IGeneratorReadOnlyContextV1.IGeneratorContext
    {
        new static sealed int IVersion => 2;
        new ILabOrderRWInterface LabOrder { get; }

        // List<Patient> Patients;

    }
}

namespace IGeneratorContext_V3
{
    [MetaDataIGeneratorIntrfc(version: 3)]
    public interface IGeneratorContext : IGeneratorContext_V2.IGeneratorContext
    {
        new static sealed int IVersion => 3;
        new ILabOrderInterfaceV2 LabOrder { get; }
    }
}


namespace IGeneratorContext_V4
{
    [MetaDataIGeneratorIntrfc(version: 4)]
    public interface IGeneratorContext : IGeneratorContext_V3.IGeneratorContext
    {
        new static sealed int IVersion => 4;
        new ILabOrderInterfaceV3 LabOrder { get; }
    }
}
namespace IGeneratorContextNoInheritance_V5
{
    [MetaDataIGeneratorIntrfc(version: 5)]
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
    [MetaDataGeneratorClass(version: 1)]
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

        public override Ember.Scripting.GeneratorContext Downgrade()
        {
            // return null;
            throw new Exception("Can not instaniate the abstract base class.");
        }
    }
}

namespace RWContextV2
{
    [MetaDataGeneratorClass(version: 2)]
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

        public override Ember.Scripting.GeneratorContext Downgrade()
        {
            try
            {
                ILabOrderInterface labOrderV1 = (ILabOrderInterface)labOrder;
                Patient patientV1 = (Patient)patient;
                ConsoleLogger loggerV1 = new ConsoleLogger();
                DataAccess dataV1 = new DataAccess();
                return new ReadOnlyContextV1.GeneratorContext(labOrderV1!, patientV1!, loggerV1!, dataV1!);
            }
            catch (Exception e)
            {
                throw new Exception("Could not downgrade your Context V" + Version + " to the previous version.", e);
            }
        }
    }
}

namespace GeneratorContextV3
{
    [MetaDataGeneratorClass(version: 3)]
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
        public override Ember.Scripting.GeneratorContext Downgrade()
        {
            try
            {
                ILabOrderRWInterface labOrderV22 = (ILabOrderRWInterface)labOrderV2;
                Patient patientV2 = (Patient)patient;
                ConsoleLogger loggerV2 = new ConsoleLogger();
                DataAccess dataV2 = new DataAccess();
                return new RWContextV2.GeneratorContext(labOrderV22!, patientV2!, loggerV2!, dataV2!);
            }
            catch (Exception e)
            {
                throw new Exception("Could not downgrade your Context V" + Version + " to the previous version.", e);
            }
        }
    }
}

namespace GeneratorContextV4
{
    [MetaDataGeneratorClass(version: 4)]
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

        public override Ember.Scripting.GeneratorContext Downgrade()
        {
            try
            {
                ILabOrderInterfaceV2 labOrderV33 = (ILabOrderInterfaceV2)labOrderV3;
                Patient patientV3 = (Patient)patient;
                ConsoleLogger loggerV3 = new ConsoleLogger();
                DataAccess dataV3 = new DataAccess();
                return new GeneratorContextV3.GeneratorContext(labOrderV33!, patientV3!, loggerV3!, dataV3!);
            }
            catch (Exception e)
            {
                throw new Exception("Could not downgrade your Context V" + Version + " to the previous version.", e);
            }
        }
    }
}

namespace GeneratorContextNoInherVaccineV5
{
    [MetaDataGeneratorClass(version: 5)]
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

        public override Ember.Scripting.GeneratorContext Downgrade()
        {
            try
            {
                ILabOrderInterfaceV3 labOrderV3 = (ILabOrderInterfaceV3)LabOrder;
                Patient patient = new Patient("1", "Default", "Default", new DateTime(2010, 6, 1, 7, 47, 0), "M");
                ConsoleLogger logger = new ConsoleLogger();
                DataAccess data = new DataAccess();
                return new GeneratorContextV4.GeneratorContext(labOrderV3!, patient!, logger!, data!);
            }
            catch (Exception e)
            {
                throw new Exception("Could not downgrade your Context V" + Version + " to the previous version.", e);
            }
        }
    }
}


//context not always backwards comp, make sure right context passed factory
