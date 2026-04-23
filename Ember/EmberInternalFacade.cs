using System.Threading.Tasks;

namespace Ember.Simulation;



internal class EmberInternalFacade
{
    private IScriptManagerExtended _scriptManager;
    public EmberInternalFacade(IScriptManagerExtended scriptManager)
    {
        _scriptManager = scriptManager;

        // Automatically find all concrete classes that inherit from RecentScriptFacade
        var scriptTypes = typeof(RecentScriptFacade).Assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(RecentScriptFacade)));

        _scriptTypeMap = new Dictionary<Type, Type>();

        foreach (var type in scriptTypes)
        {
            // Find the specific interfaces (like RecentIConditionScript) this class implements
            var interfaces = type.GetInterfaces();
            foreach (var iface in interfaces)
            {
                _scriptTypeMap[iface] = type;
            }

        }
    }

    private async Task<object> BaseExecute(Guid id, RecentContext ctx, string methodName)
    {
        CustomerScript script = await _scriptManager.GetScript(id);
        Context context = await CreateByDowngrade(script.ScriptApiVersion, ctx);
        return await _scriptManager.ExecuteScript(id, context, methodName: methodName);
    }
    private object CheckUpgradeActionResult(object result)
    {
        if (result is IUpgradeableReturnValue upgradeableReturnValue)
        {
            return UpgradeManager.UpgradeCustomReturn(result);
        }
        return result;
    }
    internal async Task<object> ExecuteScript(Guid id, RecentContext ctx, string methodName)
    {
        var result = await BaseExecute(id, ctx, methodName);
        return CheckUpgradeActionResult(result);
    }
    internal async Task<object> ExecuteScript<ScriptType>(string name, RecentContext ctx, string methodName)
    where ScriptType : IScriptType
    {
        Guid id = await _scriptManager.GetScriptId<ScriptType>(name);
        return await ExecuteScript(id, ctx, methodName);
    }
    public async Task<object> ExecuteUnfinishedScriptBySourceCode(string sourceCode, RecentContext context, string methodName)
    {
        ValidationRecord vali = _scriptManager.BasicValidationBeforeCompiling(sourceCode);
        Context ctx = await CreateByDowngrade(vali.Version, context);

        var result = await _scriptManager.ExecuteUnfinishedScriptBySourceCode(sourceCode, ctx, methodName: methodName);

        return CheckUpgradeActionResult(result);
    }
    // Original version
    public TScript GetScriptV1<TScript>(string name) where TScript : RecentScriptFacade
    {
        if (typeof(RecentIConditionScript).IsAssignableFrom(typeof(TScript)))
        {
            var toReturn = (RecentScriptFacade)new ConditionScript(_scriptManager, name);
            return (TScript)toReturn;
        }
        else if (typeof(RecentIActionScript).IsAssignableFrom(typeof(TScript)))
        {
            var toReturn = (RecentScriptFacade)new ActionScript(_scriptManager, name);
            return (TScript)toReturn;
        }
        throw new ArgumentException($"Unsupported script type: {typeof(TScript).Name}");
    }

    // //Ai generated better version of the method above
    private readonly Dictionary<Type, Func<IScriptManagerExtended, string, RecentScriptFacade>> _scriptFactories = new()
    {
    { typeof(RecentIConditionScript), (manager, name) => new ConditionScript(manager, name) },
    { typeof(RecentIActionScript), (manager, name) => new ActionScript(manager, name) }
    };

    public TScript GetScriptV2<TScript>(string name) where TScript : RecentScriptFacade
    {
        // Uses LINQ to find the first factory whose key (interface) is assignable from TScript
        var match = _scriptFactories.FirstOrDefault(kvp => kvp.Key.IsAssignableFrom(typeof(TScript)));

        if (match.Value != null)
        {
            return (TScript)match.Value(_scriptManager, name);
        }

        throw new ArgumentException($"Unsupported script type: {typeof(TScript).Name}");
    }

    // Another Ai generated version of the GetScrip Method using Reflection note the stuff inside the constructor is also part of it
    private readonly Dictionary<Type, Type> _scriptTypeMap;
    public TScript GetScript<TScript>(string name) where TScript : RecentScriptFacade
    {
        Type requestedType = typeof(TScript);

        // Find the first interface that TScript implements which we have a mapped class for
        var matchingInterface = _scriptTypeMap.Keys.FirstOrDefault(i => i.IsAssignableFrom(requestedType));

        if (matchingInterface != null)
        {
            Type concreteType = _scriptTypeMap[matchingInterface];

            // Use Activator to instantiate the class with parameters
            var instance = Activator.CreateInstance(concreteType, _scriptManager, name);
            return (TScript)instance!;
        }

        throw new ArgumentException($"Unsupported script type: {requestedType.Name}");
    }

    public async Task<TScript> CreateScript<TScript>(string sourceCode) where TScript : RecentScriptFacade
    {
        CustomerScript script = await _scriptManager.CreateScript(sourceCode);
        return GetScript<TScript>(script.ScriptName!);
    }
    public static async Task<Context> CreateByDowngrade(int desiredVersion, RecentContext ctx)  //make internal probably
    {
        return await ContextManager.CreateByDowngrade(desiredVersion, ctx);
    }
}
