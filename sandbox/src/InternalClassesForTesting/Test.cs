using Ember.Scripting;

public interface ITestInfo
{
    string TestCode { get; }
    string Name { get; }
    string Status { get; } // "Pending", "Completed", "Validated"
    string? Result { get; }
}

public class Test : ITestInfo
{
    public string TestCode { get; private set; }
    public string Name { get; private set; }
    public string Status { get; set; }
    public string? Result { get; set; }

    public Test(string code, string name)
    {
        TestCode = code;
        Name = name;
        Status = "Pending";
        Result = null;
    }
}
