using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Ember.Scripting;

namespace Sandbox;

//showcases diffrent ways of how we could have solved it otherwise, no read use case as we chose other solutions (Downgrade)
public class ContextManagementDemos
{
    private readonly IScriptManagerDeleteAfter _scriptManager;
    public ContextManagementDemos(IScriptManagerDeleteAfter scriptManager)
    {
        _scriptManager = scriptManager;
    }

    public Ember.Scripting.GeneratorContextSF CreateContextForApiV(ActiveDataClass data, int? apiV = null)
    {
        if (apiV == null)
        {
            apiV = _scriptManager.GetRunningApiVersion();
        }

        // MockData data = new MockData(labOrder: obj.labOrder, patient: obj.patient, consoleLogger: obj.logger,
        // dataAccess: obj.testDataAccess, vaccine: obj.vaccine);
        Ember.Scripting.GeneratorContextSF ctx = CreateUsingData((int)apiV, data);

        return ctx;
    }
    public static Ember.Scripting.GeneratorContextSF CreateUsingData(int version, ActiveDataClass data)
    {
        ActiveDataClass mockData = (ActiveDataClass)data;

        Dictionary<int, Type> retrievedDict = ContextVersionScanner.GetClassDictionary();
        if (retrievedDict.Keys.Contains(version) == false)
        {
            throw new Exception(message: "The version was not found in the Dictionary");
        }
        Type neededType = retrievedDict[version];
        Ember.Scripting.GeneratorContextSF uninitializedContext = (Ember.Scripting.GeneratorContextSF)RuntimeHelpers.GetUninitializedObject(neededType);
        var ctx = uninitializedContext.CreateUsingData(data);
        return ctx;
    }

}
