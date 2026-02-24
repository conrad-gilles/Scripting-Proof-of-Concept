using Microsoft.Extensions.Logging;

/// <summary>
/// Interfaces first classes are defined a bit down in the file,
/// There is one empty interface IGeneratorBaseInterface, and one empty abstract class GeneratorContext
/// </summary>
public interface IGeneratorBaseInterface { }

public interface IGeneratorReadOnlyContext : IGeneratorBaseInterface
{
    ILabOrderInterface LabOrder { get; }      // Read current order data
    PatientInterface Patient { get; }     // Read patient data
    ConsoleLoggerInterface Logger { get; }
    DataAccessInterface Data { get; }
}

public interface IGeneratorContext : IGeneratorReadOnlyContext
{
    new ILabOrderRWInterface LabOrder { get; }

}
public interface IGeneratorContext_V2 : IGeneratorContext
{
    new ILabOrderInterfaceV2 LabOrder { get; }
}

public interface IGeneratorContext_V3 : IGeneratorContext_V2
{
    new ILabOrderInterfaceV3 LabOrder { get; }
}
public interface IGeneratorContextNoInheritance_V4 : IGeneratorBaseInterface
{
    new ILabOrderInterfaceV4NoInheritence LabOrder { get; }
    new IVaccineInterface Vaccine { get; }
}

/// <summary>
/// Classes of context come here
/// </summary>
public abstract class GeneratorContext { }
public class ReadOnlyContext : GeneratorContext, IGeneratorReadOnlyContext
{
    public ILabOrderInterface labOrder;
    public PatientInterface patient;

    public ConsoleLoggerInterface logger;
    public DataAccessInterface data;


    public ReadOnlyContext(ILabOrderInterface pLabOrder, PatientInterface pPatient, ConsoleLoggerInterface plogger, DataAccessInterface pdata)
    {
        labOrder = pLabOrder;
        patient = pPatient;
        logger = plogger;
        data = pdata;

    }
    ILabOrderInterface IGeneratorReadOnlyContext.LabOrder => labOrder;
    PatientInterface IGeneratorReadOnlyContext.Patient => patient;
    ConsoleLoggerInterface IGeneratorReadOnlyContext.Logger => logger;
    DataAccessInterface IGeneratorReadOnlyContext.Data => data;
}

public class RWContext : GeneratorContext, IGeneratorContext
{
    public ILabOrderRWInterface labOrder;
    public PatientInterface patient;

    public ConsoleLoggerInterface logger;
    public DataAccessInterface data;


    public RWContext(ILabOrderRWInterface pLabOrder, PatientInterface pPatient, ConsoleLoggerInterface plogger, DataAccessInterface pdata)
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

    ILabOrderInterface IGeneratorReadOnlyContext.LabOrder => LabOrder;
}
public class GeneratorContextV2 : RWContext, IGeneratorContext_V2
{
    public ILabOrderInterfaceV2 labOrderV2;
    public PatientInterface patient;

    public ConsoleLoggerInterface logger;
    public DataAccessInterface data;


    public GeneratorContextV2(ILabOrderInterfaceV2 pLabOrder, PatientInterface pPatient, ConsoleLoggerInterface plogger, DataAccessInterface pdata)
    : base(pLabOrder, pPatient, plogger, pdata)
    {
        labOrderV2 = pLabOrder;
        patient = pPatient;
        logger = plogger;
        data = pdata;

    }
    ILabOrderInterfaceV2 IGeneratorContext_V2.LabOrder => labOrderV2;

}
public class GeneratorContextV3 : GeneratorContextV2, IGeneratorContext_V3  //todo implement Adapter pattern
{
    public ILabOrderInterfaceV3 labOrderV3;
    public PatientInterface patient;

    public ConsoleLoggerInterface logger;
    public DataAccessInterface data;


    public GeneratorContextV3(ILabOrderInterfaceV3 pLabOrderV3, PatientInterface pPatient, ConsoleLoggerInterface plogger, DataAccessInterface pdata)
    : base(pLabOrderV3, pPatient, plogger, pdata)
    {
        labOrderV3 = pLabOrderV3;
        patient = pPatient;
        logger = plogger;
        data = pdata;

    }
    ILabOrderInterfaceV3 IGeneratorContext_V3.LabOrder => labOrderV3;

}

public class GeneratorContextNoInherVaccine : GeneratorContext, IGeneratorContextNoInheritance_V4
{
    ILabOrderInterfaceV4NoInheritence LabOrder;
    IVaccineInterface Vaccine;
    public GeneratorContextNoInherVaccine(ILabOrderInterfaceV4NoInheritence labOrder, IVaccineInterface vaccine)
    {
        LabOrder = labOrder;
        Vaccine = vaccine;
    }

    ILabOrderInterfaceV4NoInheritence IGeneratorContextNoInheritance_V4.LabOrder => LabOrder;

    IVaccineInterface IGeneratorContextNoInheritance_V4.Vaccine => Vaccine;
}

//context not always backwards comp, make sure right context passed factory 