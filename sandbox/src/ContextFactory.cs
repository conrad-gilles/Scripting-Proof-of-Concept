using System.Runtime.CompilerServices;
using Ember.Scripting;

public static class ContextFactory
{
    public static GeneratorContext CreateContext(int version, DataBaseClass data)
    {
        MockData mockData = (MockData)data;

        Dictionary<int, Type> retrievedDict = ContextVersionScanner.GetClassDictionary();
        if (retrievedDict.Keys.Contains(version) == false)
        {
            throw new Exception(message: "The version was not found in the Dictionary");
        }
        Type neededType = retrievedDict[version];
        GeneratorContext uninitializedContext = (Ember.Scripting.GeneratorContext)RuntimeHelpers.GetUninitializedObject(neededType);
        var ctx = uninitializedContext.CreateUsingData(data);
        return ctx;
    }
}