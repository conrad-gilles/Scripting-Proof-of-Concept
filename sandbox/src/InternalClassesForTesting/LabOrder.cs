using System;
using System.Collections.Generic;
using System.Linq;
using Ember.Scripting;

// Interface for Condition Scripts (Read Only)
public interface ILabOrderInterface
{
    string OrderNumber { get; }
    string Department { get; }
    string Priority { get; }
    DateTime ReceivedDate { get; }
    IReadOnlyList<ITestInfo> Tests { get; }
    bool HasTest(string testCode);
    object? GetCustomField(string name);
}
// Interface for Action Scripts (Read/Write)
public interface ILabOrderRWInterface : ILabOrderInterface
{
    void SetCustomField(string name, object value);
    bool AddTest(string testCode);
    void SetPriority(string priority);
    void RemoveTest(string testCode);
}

// V2 adds functionality
public interface ILabOrderInterfaceV2 : ILabOrderRWInterface
{
    double RandomNewDouble { get; set; }       //new variable added in new Interface version of LabOrder 2.
}

// V3 adds async operations
public interface ILabOrderInterfaceV3 : ILabOrderInterfaceV2
{
    string RandomNewFunctionInV3(string funcName);
}
public interface ILabOrderInterfaceV4NoInheritence  //where acces gets defined in get set
{
    string OrderNumber { get; }
    string Department { get; }
    bool AddTest(string testCode);
    string RandomNewFunctionInV3(string funcName);
}
// Full Implementation
internal class LabOrder : ILabOrderRWInterface, ILabOrderInterface, ILabOrderInterfaceV2, ILabOrderInterfaceV3, ILabOrderInterfaceV4NoInheritence    //todo issues has to implement loads of interfaces, test if removing one is fine
{
    public string OrderNumber { get; private set; }
    public string Department { get; private set; } // e.g., "Pediatrics", "Cardiology"
    public string Priority { get; private set; }   // e.g., "Routine", "Stat"
    public DateTime ReceivedDate { get; private set; }
    public double RandomNewDouble { get; set; } //random new double to test api versioning 1.

    private List<Test> _tests = new List<Test>();
    private Dictionary<string, object> _customFields = new Dictionary<string, object>();

    // Explicit interface implementation to expose list as IReadOnlyList<ITestInfo>
    public IReadOnlyList<ITestInfo> Tests => _tests.Cast<ITestInfo>().ToList();

    public LabOrder(string orderNumber, string department)
    {
        OrderNumber = orderNumber;
        Department = department;
        Priority = "Routine"; // Default
        ReceivedDate = DateTime.Now;
        RandomNewDouble = 9.9;
    }



    public bool HasTest(string testCode)
    {
        return _tests.Any(t => t.TestCode.Equals(testCode, StringComparison.OrdinalIgnoreCase));
    }

    public bool AddTest(string testCode)
    {
        if (HasTest(testCode)) return false; // Already exists

        _tests.Add(new Test(testCode, $"Test {testCode}"));
        return true;
    }

    public void RemoveTest(string testCode)
    {
        var test = _tests.FirstOrDefault(t => t.TestCode.Equals(testCode, StringComparison.OrdinalIgnoreCase));
        if (test != null) _tests.Remove(test);
    }

    public void SetPriority(string priority)
    {

        Priority = priority;
    }

    public void SetCustomField(string name, object value)
    {
        _customFields[name] = value;
    }

    public object? GetCustomField(string name)
    {
        return _customFields.ContainsKey(name) ? _customFields[name] : null;
    }
    public string RandomNewFunctionInV3(string funcName)
    {
        Console.WriteLine("Hello " + funcName + " !");
        return "Hello " + funcName + " !";
    }
}
