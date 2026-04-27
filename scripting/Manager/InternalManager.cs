namespace Ember.Scripting.Manager;

public class InternalManager : IInternalManager
{
    private IScriptManagerExtended _scriptManager;
    private readonly Dictionary<Type, Type> _scriptTypeMap;

    public InternalManager(IScriptManagerExtended scriptManager)
    {
        _scriptManager = scriptManager;

        // Automatically find all concrete classes that inherit from RecentScriptFacade
        var scriptTypes = AppDomain.CurrentDomain.GetAssemblies()
        .SelectMany(assembly => VersionScannerHelper.GetLoadableTypes(assembly))
        .Where(t => t.IsClass
             && !t.IsAbstract
             && typeof(RecentScriptFacade).IsAssignableFrom(t));

        _scriptTypeMap = new Dictionary<Type, Type>();

        foreach (var type in scriptTypes)
        {
            // Find the specific interfaces (like RecentIConditionScript) this class implements
            var interfaces = type.GetInterfaces();
            foreach (var iface in interfaces)
            {
                if (iface == typeof(RecentScriptFacade))
                {
                    continue;
                }
                _scriptTypeMap[iface] = type;
            }

        }
    }

    private async Task<object> BaseExecute(Guid id, IRecentContext ctx, string methodName)
    {
        CustomerScript script = await _scriptManager.GetScript(id);
        IContext context = (IContext)ContextManager.CreateByDowngrade(script.ScriptApiVersion, ctx);
        return await _scriptManager.ExecuteScript(id, (IContext)context, methodName: methodName);
    }
    private object CheckUpgradeResult(object result)
    {
        if (result is IUpgradeableReturnValue upgradeableReturnValue)
        {
            return UpgradeManager.UpgradeReturnValue(result);
        }
        return result;
    }
    public async Task<object> ExecuteScript(Guid id, IRecentContext context, string methodName)
    {
        var result = await BaseExecute(id, context, methodName);
        return CheckUpgradeResult(result);
    }
    public async Task<object> ExecuteScript<ScriptType>(string name, IRecentContext context, string methodName)
    where ScriptType : IScriptType
    {
        Guid id = await _scriptManager.GetScriptId<ScriptType>(name);
        return await ExecuteScript(id, context, methodName);
    }
    public async Task<object> ExecuteUnfinishedScriptBySourceCode(string sourceCode, IRecentContext context, string methodName)
    {
        ValidationRecord vali = _scriptManager.BasicValidationBeforeCompiling(sourceCode);
        IContext ctx = (IContext)ContextManager.CreateByDowngrade(vali.Version, context);

        var result = await _scriptManager.ExecuteUnfinishedScriptBySourceCode(sourceCode, (IContext)ctx, methodName: methodName);

        return CheckUpgradeResult(result);
    }

    //Ai generated, with also the bottom part of the Constructor
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
}

public interface IInternalManager
{
    Task<object> ExecuteScript(Guid id, IRecentContext context, string methodName);
    Task<object> ExecuteScript<ScriptType>(string name, IRecentContext context, string methodName)
        where ScriptType : IScriptType;
    Task<object> ExecuteUnfinishedScriptBySourceCode(string sourceCode, IRecentContext context, string methodName);
    TScript GetScript<TScript>(string name) where TScript : RecentScriptFacade;
    Task<TScript> CreateScript<TScript>(string sourceCode) where TScript : RecentScriptFacade;
}