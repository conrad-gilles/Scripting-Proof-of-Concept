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
    /// <summary>
    /// Gets the context version for the running Ember System
    /// </summary>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    /// <exception cref="ArgumentException"></exception>
    public GeneratorContext CreateContext()
    {
        // int apiV = EmberMethods.GetEmberApiVersion(ScriptManager.GetRecentApiVersion());
        int apiV = ScriptManager.GetRunningApiVersion();

        Type recentType;
        Dictionary<int, Type> contextVersionMap = ContextVersionScanner.GetClassDictionary();

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
            var t when t == typeof(ReadOnlyContextV1.GeneratorContext) => new ReadOnlyContextV1.GeneratorContext(objs.labOrder, objs.patient, objs.logger, objs.testDataAccess),
            var t when t == typeof(RWContextV2.GeneratorContext) => new RWContextV2.GeneratorContext(objs.labOrder, objs.patient, objs.logger, objs.testDataAccess),
            var t when t == typeof(GeneratorContextV3.GeneratorContext) => new GeneratorContextV3.GeneratorContext(objs.labOrder, objs.patient, objs.logger, objs.testDataAccess),
            var t when t == typeof(GeneratorContextV4.GeneratorContext) => new GeneratorContextV4.GeneratorContext(objs.labOrder, objs.patient, objs.logger, objs.testDataAccess),
            var t when t == typeof(GeneratorContextNoInherVaccineV5.GeneratorContext) => new GeneratorContextNoInherVaccineV5.GeneratorContext(objs.labOrder, objs.vaccine),
            _ => throw new ArgumentException($"Unsupported context type: {recentType.Name}")
        };

        return ctx;
    }


    public GeneratorContext CreateContextForApiV(int? apiV = null)
    {
        if (apiV == null)
        {
            apiV = ScriptManager.GetRunningApiVersion();
        }
        // Type recentType;
        // Dictionary<int, Type> contextVersionMap = ContextVersionScanner.GetClassDictionary();

        // if (contextVersionMap.Keys.Contains((int)apiV) == false)
        // {
        //     throw new Exception("No Context class defined in " + nameof(contextVersionMap) + " for the passed API version.");
        //     // might be better to instead return latest version?
        //     // recentType = contextVersionMap.Last().Value;
        // }
        // recentType = contextVersionMap[(int)apiV];
        var obj = ScriptObjects();

        // GeneratorContext ctx = recentType switch
        // {
        //     var t when t == typeof(ReadOnlyContextV1.GeneratorContext) => new ReadOnlyContextV1.GeneratorContext(obj.labOrder, obj.patient, obj.logger, obj.testDataAccess),
        //     var t when t == typeof(RWContextV2.GeneratorContext) => new RWContextV2.GeneratorContext(obj.labOrder, obj.patient, obj.logger, obj.testDataAccess),
        //     var t when t == typeof(GeneratorContextV3.GeneratorContext) => new GeneratorContextV3.GeneratorContext(obj.labOrder, obj.patient, obj.logger, obj.testDataAccess),
        //     var t when t == typeof(GeneratorContextV4.GeneratorContext) => new GeneratorContextV4.GeneratorContext(obj.labOrder, obj.patient, obj.logger, obj.testDataAccess),
        //     var t when t == typeof(GeneratorContextNoInherVaccineV5.GeneratorContext) => new GeneratorContextNoInherVaccineV5.GeneratorContext(obj.labOrder, obj.vaccine),
        //     _ => throw new ArgumentException($"Unsupported context type: {recentType.Name}")
        // };

        MockData data = new MockData(labOrder: obj.labOrder, patient: obj.patient, consoleLogger: obj.logger,
        dataAccess: obj.testDataAccess, vaccine: obj.vaccine);
        GeneratorContext ctx = ContextFactory.CreateContext((int)apiV, data);


        return ctx;
    }
    public async Task<GeneratorContext> CreateContextByDowngrade(string sourceCode)
    {
        var vali = ScriptManager.BasicValidationBeforeCompiling(sourceCode);
        int? apiV = null;
        if (apiV == null)
        {
            apiV = ScriptManager.GetRunningApiVersion();
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
        Type desiredType = contextVersionMap[vali.versionInt];
        var objs = ScriptObjects();
        GeneratorContext context = CreateContextForApiV();
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
        return (GeneratorContext)context;
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