using System;
using System.Collections.Generic;
using System.Linq;

// Interface for Action Scripts (Read/Write)
public interface ILabOrderRWInterface : ILabOrderInterface
{
    void SetCustomField(string name, object value);
    bool AddTest(string testCode);
    void SetPriority(string priority);
    void RemoveTest(string testCode);
}

// Interface for Condition Scripts (Read Only)
public interface ILabOrderInterface
{
    string OrderNumber { get; }
    string Department { get; }
    string Priority { get; }
    DateTime ReceivedDate { get; }
    IReadOnlyList<ITestInfo> Tests { get; }
    bool HasTest(string testCode);
    object GetCustomField(string name);
}
// V2 adds functionality
public interface ILabOrderInterfaceV2 : ILabOrderRWInterface
{
    double RandomNewDouble { get; set; }       //new variable added in new Interface version of LabOrder 2.
}

// V3 adds async operations
public interface ILabOrderInterfaceV3 : ILabOrderInterfaceV2
{
    string randomNewFunctionInV3(string funcName);
}
// Full Implementation
internal class LabOrder : ILabOrderRWInterface, ILabOrderInterface, ILabOrderInterfaceV2, ILabOrderInterfaceV3    //todo issues has to implement loads of interfaces, test if removing one is fine
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

        // In a real system, you'd look up the test definition here. 
        // For PoC, we just create a generic test.
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
        // Add validation logic if needed
        Priority = priority;
    }

    public void SetCustomField(string name, object value)
    {
        _customFields[name] = value;
    }

    public object GetCustomField(string name)
    {
        return _customFields.ContainsKey(name) ? _customFields[name] : null;
    }
    public string randomNewFunctionInV3(string funcName)
    {
        Console.WriteLine("Hello " + funcName + " !");
        return "Hello " + funcName + " !";
    }
}
