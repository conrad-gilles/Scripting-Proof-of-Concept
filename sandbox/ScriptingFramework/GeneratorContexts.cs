using TypeInfo = Ember.Scripting.Versioning.TypeInfo;
using Microsoft.Extensions.Logging;
using Ember.Scripting;
using IGeneratorReadOnlyContextV1;
using ContextBases;

namespace ContextBases
{
    [MetaDataIGeneratorIntrfc(version: 0, type: TypeInfo.AbstractBaseInSF)]
    public interface IGeneratorContextBaseInterfaceSF : IContext
    {
    }
    [MetaDataGeneratorClass(version: 0, type: TypeInfo.AbstractBaseInSF)]
    public abstract class GeneratorContextSF : Context, IContext
    {

    }
}

namespace IGeneratorReadOnlyContextV1
{
    [MetaDataIGeneratorIntrfc(version: 1)]
    public interface IGeneratorContext : IGeneratorContextBaseInterfaceSF
    {
        ILabOrderInterface LabOrder { get; }      // Read current order data
        IPatientInterface Patient2 { get; }     // Read patient data
        IConsoleLoggerInterface Logger2 { get; }
        IDataAccessInterface Data2 { get; }
    }
}

namespace IGeneratorContext_V2
{
    [MetaDataIGeneratorIntrfc(version: 2)]
    public interface IGeneratorContext : IGeneratorReadOnlyContextV1.IGeneratorContext
    {
        new ILabOrderRWInterface LabOrder2 { get; }

        // List<Patient> Patients;

    }
}

namespace IGeneratorContext_V3
{
    [MetaDataIGeneratorIntrfc(version: 3)]
    public interface IGeneratorContext : IGeneratorContext_V2.IGeneratorContext
    {
        new ILabOrderInterfaceV2 LabOrder { get; }
    }
}


namespace IGeneratorContext_V4
{
    [MetaDataIGeneratorIntrfc(version: 4)]
    public interface IGeneratorContext : IGeneratorContext_V3.IGeneratorContext
    {
        new ILabOrderInterfaceV3 LabOrder { get; }
    }
}
namespace IGeneratorContextNoInheritance_V5
{
    [MetaDataIGeneratorIntrfc(version: 5)]
    public interface IGeneratorContext : IGeneratorContextBaseInterfaceSF
    {
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
    public class GeneratorContext : GeneratorContextSF, IGeneratorReadOnlyContextV1.IGeneratorContext
    {
        public ILabOrderInterface LabOrder;
        public IPatientInterface Patient;

        public IConsoleLoggerInterface Logger;
        public IDataAccessInterface Data;

        public GeneratorContext(ILabOrderInterface labOrder, IPatientInterface patient, IConsoleLoggerInterface logger, IDataAccessInterface data)
        {
            LabOrder = labOrder;
            Patient = patient;
            Logger = logger;
            Data = data;

        }

        ILabOrderInterface IGeneratorReadOnlyContextV1.IGeneratorContext.LabOrder => LabOrder;
        IPatientInterface IGeneratorReadOnlyContextV1.IGeneratorContext.Patient2 => Patient;
        IConsoleLoggerInterface IGeneratorReadOnlyContextV1.IGeneratorContext.Logger2 => Logger;
        IDataAccessInterface IGeneratorReadOnlyContextV1.IGeneratorContext.Data2 => Data;
        public override GeneratorContextSF Downgrade()
        {
            // return null;
            throw new Exception("Can not instaniate the abstract base class.");
        }
    }
}

namespace RWContextV2
{
    [MetaDataGeneratorClass(version: 2)]
    public class GeneratorContext : GeneratorContextSF, IGeneratorContext_V2.IGeneratorContext
    {
        public ILabOrderRWInterface LabOrder;
        public IPatientInterface Patient;

        public IConsoleLoggerInterface Logger;
        public IDataAccessInterface Data;


        public GeneratorContext(ILabOrderRWInterface labOrder, IPatientInterface patient, IConsoleLoggerInterface logger, IDataAccessInterface data)
        {
            LabOrder = labOrder;
            Patient = patient;
            Logger = logger;
            Data = data;

        }

        public ILabOrderRWInterface LabOrder2 => LabOrder;

        public IPatientInterface Patient2 => Patient;

        public IConsoleLoggerInterface Logger2 => Logger;

        public IDataAccessInterface Data2 => Data;

        ILabOrderInterface IGeneratorReadOnlyContextV1.IGeneratorContext.LabOrder => LabOrder2;
        public override GeneratorContextSF Downgrade()
        {
            try
            {
                ILabOrderInterface labOrderV1 = (ILabOrderInterface)LabOrder;
                Patient patientV1 = (Patient)Patient;
                ConsoleLogger loggerV1 = new ConsoleLogger();
                DataAccess dataV1 = new DataAccess();
                return new ReadOnlyContextV1.GeneratorContext(labOrderV1!, patientV1!, loggerV1!, dataV1!);
            }
            catch (Exception e)
            {
                throw new Exception("Could not downgrade your Context to the previous version.", e);
            }
        }
    }
}

namespace GeneratorContextV3
{
    [MetaDataGeneratorClass(version: 3)]
    public class GeneratorContext : RWContextV2.GeneratorContext, IGeneratorContext_V3.IGeneratorContext
    {
        public ILabOrderInterfaceV2 LabOrderV2;
        public new IPatientInterface Patient;

        public new IConsoleLoggerInterface Logger;
        public new IDataAccessInterface Data;


        public GeneratorContext(ILabOrderInterfaceV2 labOrder, IPatientInterface patient, IConsoleLoggerInterface logger, IDataAccessInterface data)
        : base(labOrder, patient, logger, data)
        {
            LabOrderV2 = labOrder;
            Patient = patient;
            Logger = logger;
            Data = data;

        }
        ILabOrderInterfaceV2 IGeneratorContext_V3.IGeneratorContext.LabOrder => LabOrderV2;

        public override GeneratorContextSF Downgrade()
        {
            try
            {
                ILabOrderRWInterface labOrderV22 = (ILabOrderRWInterface)LabOrderV2;
                Patient patientV2 = (Patient)Patient;
                ConsoleLogger loggerV2 = new ConsoleLogger();
                DataAccess dataV2 = new DataAccess();
                return new RWContextV2.GeneratorContext(labOrderV22!, patientV2!, loggerV2!, dataV2!);
            }
            catch (Exception e)
            {
                throw new Exception("Could not downgrade your Context to the previous version.", e);
            }
        }
    }
}

namespace GeneratorContextV4
{
    [MetaDataGeneratorClass(version: 4)]
    public class GeneratorContext : GeneratorContextV3.GeneratorContext, IGeneratorContext_V4.IGeneratorContext  //todo implement Adapter pattern
    {
        public ILabOrderInterfaceV3 LabOrderV3;
        public new IPatientInterface Patient;

        public new IConsoleLoggerInterface Logger;
        public new IDataAccessInterface Data;


        public GeneratorContext(ILabOrderInterfaceV3 labOrderV3, IPatientInterface patient, IConsoleLoggerInterface logger, IDataAccessInterface data)
        : base(labOrderV3, patient, logger, data)
        {
            LabOrderV3 = labOrderV3;
            Patient = patient;
            Logger = logger;
            Data = data;

        }
        ILabOrderInterfaceV3 IGeneratorContext_V4.IGeneratorContext.LabOrder => LabOrderV3;

        public override GeneratorContextSF Downgrade()
        {
            try
            {
                ILabOrderInterfaceV2 labOrderV33 = (ILabOrderInterfaceV2)LabOrderV3;
                Patient patientV3 = (Patient)Patient;
                ConsoleLogger loggerV3 = new ConsoleLogger();
                DataAccess dataV3 = new DataAccess();
                return new GeneratorContextV3.GeneratorContext(labOrderV33!, patientV3!, loggerV3!, dataV3!);
            }
            catch (Exception e)
            {
                throw new Exception("Could not downgrade your Context to the previous version.", e);
            }
        }
    }
}

namespace GeneratorContextNoInherVaccineV5
{
    [MetaDataGeneratorClass(version: 5)]
    public class GeneratorContext : GeneratorContextSF, IGeneratorContextNoInheritance_V5.IGeneratorContext   //into diffrent namespaces blocks later folders
    {
        ILabOrderInterfaceV4NoInheritence LabOrder;
        IVaccineInterface Vaccine;
        public GeneratorContext(ILabOrderInterfaceV4NoInheritence labOrder, IVaccineInterface vaccine)
        {
            LabOrder = labOrder;
            Vaccine = vaccine;
        }

        ILabOrderInterfaceV4NoInheritence IGeneratorContextNoInheritance_V5.IGeneratorContext.LabOrder => LabOrder;

        IVaccineInterface IGeneratorContextNoInheritance_V5.IGeneratorContext.Vaccine => Vaccine;

        public override GeneratorContextSF Downgrade()
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
                throw new Exception("Could not downgrade your Context to the previous version.", e);
            }
        }
    }
}

// namespace GeneratorContextV6
// {
//     [MetaDataGeneratorClass(version: -1)]
//     public class GeneratorContext : GeneratorContextNoInherVaccineV5.GeneratorContext //into diffrent namespaces blocks later folders
//     {
//         public void DoIt()
//         {

//         }
//         public GeneratorContext(ILabOrderInterfaceV4NoInheritence labOrder, IVaccineInterface vaccine) : base(labOrder, vaccine)
//         {
//         }
//     }
// }


//context not always backwards comp, make sure right context passed factory
