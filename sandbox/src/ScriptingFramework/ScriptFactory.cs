using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Ember.Scripting;

public class ScriptFactory
{
    private readonly ISccriptManagerDeleteAfter ScriptManager;
    public ScriptFactory(ISccriptManagerDeleteAfter scriptManager)
    {
        ScriptManager = scriptManager;
    }
    // internal static readonly Dictionary<int, Type> contextVersionMap = new()
    //     {
    //         {0, typeof(GeneratorContext)},
    //         {1, typeof(ReadOnlyContext.GeneratorContext)},
    //         {2, typeof(RWContext.GeneratorContext)},
    //         {3, typeof(GeneratorContextV2.GeneratorContext)},
    //         {4, typeof(GeneratorContextV3.GeneratorContext)},
    //         {5, typeof(GeneratorContextNoInherVaccine.GeneratorContext)},
    //     };
    public async Task<GeneratorContext> CreateContext()
    {
        // int apiV = EmberMethods.GetEmberApiVersion(ScriptManager.GetRecentApiVersion());
        int apiV = await ScriptManager.GetRunningApiVersion();

        Type recentType;
        Dictionary<int, Type> contextVersionMap = GetDictionary();

        if (contextVersionMap.Keys.Contains(apiV) == false)
        {
            throw new Exception("No Context class defined in " + nameof(contextVersionMap) + " for the passed API version.");
            // might be better to instead return latest version?
            // recentType = contextVersionMap.Last().Value;
        }
        recentType = contextVersionMap[apiV];
        var objs = ScriptObjects();

        GeneratorContext ctx = recentType switch
        {
            var t when t == typeof(ReadOnlyContext.GeneratorContext) => new ReadOnlyContext.GeneratorContext(objs.labOrder, objs.patient, objs.logger, objs.testDataAccess),
            var t when t == typeof(RWContext.GeneratorContext) => new RWContext.GeneratorContext(objs.labOrder, objs.patient, objs.logger, objs.testDataAccess),
            var t when t == typeof(GeneratorContextV2.GeneratorContext) => new GeneratorContextV2.GeneratorContext(objs.labOrder, objs.patient, objs.logger, objs.testDataAccess),
            var t when t == typeof(GeneratorContextV3.GeneratorContext) => new GeneratorContextV3.GeneratorContext(objs.labOrder, objs.patient, objs.logger, objs.testDataAccess),
            var t when t == typeof(GeneratorContextNoInherVaccine.GeneratorContext) => new GeneratorContextNoInherVaccine.GeneratorContext(objs.labOrder, objs.vaccine),
            _ => throw new ArgumentException($"Unsupported context type: {recentType.Name}")
        };

        return ctx;
    }

    private (LabOrder labOrder, Patient patient, ConsoleLogger logger, DataAccess testDataAccess, Vaccine vaccine) ScriptObjects()
    {
        LabOrder labOrder = new LabOrder("1", "Pediatrics");
        Patient patient = new Patient("1", "TestFirst", "TestLast", new DateTime(2010, 6, 1, 7, 47, 0), "M");   //mfu
        ConsoleLogger logger = new ConsoleLogger();
        DataAccess testDataAccess = new DataAccess();
        Vaccine vaccine = new Vaccine("Polio", 1, DateTime.UtcNow);

        return (labOrder, patient, logger, testDataAccess, vaccine);
    }

    public Dictionary<int, Type> GetDictionary()
    {
        Dictionary<int, Type> contextVersionMap = new() { };

        // the following 5 lines were ai generated i simply asked how to get subclasses in c# dotnet
        Type baseType = typeof(Ember.Scripting.GeneratorContext);
        var subClasses = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(assembly => assembly.GetTypes())
            .Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(baseType))
            .ToList();

        for (int i = 0; i < subClasses.Count(); i++)
        {
            Type currentType = subClasses[i];
            var uninitializedContext = (Ember.Scripting.GeneratorContext)RuntimeHelpers.GetUninitializedObject(currentType);

            int version = uninitializedContext.Version;
            if (contextVersionMap.Values.Contains(currentType))
            {
                throw new Exception("Type was more than once in the assembly probably with more than 1 Version property");
            }
            if (contextVersionMap.Keys.Contains(version))
            {
                throw new Exception("Api version int more than once in the assembly should not happen.");
            }
            contextVersionMap.Add(version, currentType);
        }

        return contextVersionMap;
    }
}