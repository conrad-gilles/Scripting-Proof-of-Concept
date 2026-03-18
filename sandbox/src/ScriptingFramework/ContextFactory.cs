using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Ember.Scripting;

public class ContextFactory
{
    private readonly ISccriptManagerDeleteAfter _scriptManager;
    public ContextFactory(ISccriptManagerDeleteAfter scriptManager)
    {
        _scriptManager = scriptManager;
    }

    public Ember.Scripting.GeneratorContextSF CreateContextForApiV(ActiveDataClass data, int? apiV = null)
    {
        if (apiV == null)
        {
            apiV = _scriptManager.GetRunningApiVersion();
        }

        var obj = ScriptObjects();

        // MockData data = new MockData(labOrder: obj.labOrder, patient: obj.patient, consoleLogger: obj.logger,
        // dataAccess: obj.testDataAccess, vaccine: obj.vaccine);
        Ember.Scripting.GeneratorContextSF ctx = CreateUsingData((int)apiV, data);

        return ctx;
    }
    public async Task<GeneratorContextSF> CreateByDowngrade(Guid id, ActiveGeneratorContext ctx)
    // public async Task<Ember.Scripting.GeneratorContextSF> CreateByDowngrade(Guid id, ActiveDataClass data)
    // public async Task<ActiveGeneratorContext> CreateByDowngrade(string sourceCode)
    {
        CustomerScript script = await _scriptManager.GetScript(id);
        if (script.SourceCode == null)
        {
            throw new Exception();
        }
        var vali = _scriptManager.BasicValidationBeforeCompiling(script.SourceCode!);
        int? apiV = null;
        if (apiV == null)
        {
            apiV = _scriptManager.GetRunningApiVersion();
        }
        Type recentType;
        Dictionary<int, Type> contextVersionMap = ContextVersionScanner.GetClassDictionary();

        if (contextVersionMap.Keys.Contains((int)apiV) == false)
        {
            throw new Exception("No Context class defined in " + nameof(contextVersionMap) + " for the passed API version.");
            // might be better to instead return latest version?
            // recentType = contextVersionMap.Last().Value;
        }
        recentType = contextVersionMap[(int)apiV];
        Type desiredType = contextVersionMap[vali.Version];
        var objs = ScriptObjects();
        // Ember.Scripting.GeneratorContextSF context = CreateContextForApiV(data);
        Ember.Scripting.GeneratorContextSF context = ctx;
        int iterations = 0;
        int maxIterations = ContextVersionScanner.GetClassDictionary().Keys.Count() + 3;

        while (recentType != desiredType && iterations <= maxIterations)
        {
            if (recentType == typeof(GeneratorContextNoInherVaccineV5.GeneratorContext))
            {
                try
                {
                    context = context.Downgrade();
                    recentType = context.GetType();
                }
                catch (Exception e)
                {
                    throw new Exception("CreateContextByDowngrade failed in while.", e);
                }
            }
            if (iterations > maxIterations)
            {
                throw new Exception("Somethign went wrong trying to downgrade the context");
            }
            iterations++;
        }
        // return (ActiveGeneratorContext)context;
        return (GeneratorContextSF)context;
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

    internal (LabOrder labOrder, Patient patient, ConsoleLogger logger, DataAccess testDataAccess, Vaccine vaccine) ScriptObjects()
    {
        LabOrder labOrder = new LabOrder("1", "Pediatrics");
        Patient patient = new Patient("1", "TestFirst", "TestLast", new DateTime(2010, 6, 1, 7, 47, 0), "M");   //mfu
        ConsoleLogger logger = new ConsoleLogger();
        DataAccess testDataAccess = new DataAccess();
        Vaccine vaccine = new Vaccine("Polio", 1, DateTime.UtcNow);

        return (labOrder, patient, logger, testDataAccess, vaccine);
    }
}