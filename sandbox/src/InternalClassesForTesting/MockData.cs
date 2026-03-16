using Ember.Scripting;
namespace DataV1
{
    public class MockData : DataAbstractClass
    {
        internal ConsoleLogger? ConsoleLogger;
        internal DataAccess? DataAccess;
        internal LabOrder? LabOrder;
        internal Patient? Patient;
        internal Vaccine? Vaccine;
        internal Test? Test;

        internal MockData(ConsoleLogger? consoleLogger = null, DataAccess? dataAccess = null,
        LabOrder? labOrder = null, Patient? patient = null, Vaccine? vaccine = null, Test? test = null)
        {
            ConsoleLogger = consoleLogger;
            DataAccess = dataAccess;
            LabOrder = labOrder;
            Patient = patient;
            Vaccine = vaccine;
            Test = test;
        }
    }
}
namespace DataV2
{
    public class DataV2 : DataAbstractClass
    {
        internal ConsoleLogger? ConsoleLogger;
        internal DataAccess? DataAccess;
        internal LabOrder? LabOrder;
        internal Patient? Patient;
        internal Vaccine? Vaccine;
        internal Test? Test;

        internal DataV2(ConsoleLogger? consoleLogger = null, DataAccess? dataAccess = null,
        LabOrder? labOrder = null, Patient? patient = null, Vaccine? vaccine = null, Test? test = null)
        {
            ConsoleLogger = consoleLogger;
            DataAccess = dataAccess;
            LabOrder = labOrder;
            Patient = patient;
            Vaccine = vaccine;
            Test = test;
        }
    }

}
