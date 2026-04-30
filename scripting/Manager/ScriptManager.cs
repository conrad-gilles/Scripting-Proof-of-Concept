namespace Ember.Scripting.Manager;

public class ScriptManager : IScriptManager
{
    private IScriptManagerBaseExtended _scriptManager;
    private readonly Dictionary<Type, Type> _scriptTypeMap;

    public ScriptManager(IScriptManagerBaseExtended scriptManager)
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

/// <summary>
/// A simplified interface for running and managing scripts, handling context conversions automatically behind the scenes.
/// </summary>
public interface IScriptManager
{
    /// <summary>
    /// Runs a script by its ID and returns the result.
    /// </summary>
    /// <param name="id">The unique ID of the script.</param>
    /// <param name="context">The context passed to the method.</param>
    /// <param name="methodName">The method to run inside the script.</param>
    /// <returns>The upgraded result from the script.</returns>
    Task<object> ExecuteScript(Guid id, IRecentContext context, string methodName);

    /// <summary>
    /// Runs a script by its name and returns the result.
    /// </summary>
    /// <typeparam name="ScriptType">The script type.</typeparam>
    /// <param name="name">The name of the script.</param>
    /// <param name="context">The context passed to the method.</param>
    /// <param name="methodName">The method to run inside the script.</param>
    /// <returns>The upgraded result from the script.</returns>
    Task<object> ExecuteScript<ScriptType>(string name, IRecentContext context, string methodName)
        where ScriptType : IScriptType;

    /// <summary>
    /// Quickly tests and runs raw C# code without saving it permanently.
    /// </summary>
    /// <param name="sourceCode">The C# code.</param>
    /// <param name="context">The context passed to the method.</param>
    /// <param name="methodName">The method to run.</param>
    /// <returns>The upgraded result from the script.</returns>
    Task<object> ExecuteUnfinishedScriptBySourceCode(string sourceCode, IRecentContext context, string methodName);

    /// <summary>
    /// Retrieves a script facade object to easily interact with a specific script.
    /// </summary>
    /// <typeparam name="TScript">The specific script facade type.</typeparam>
    /// <param name="name">The name of the script.</param>
    /// <returns>The script facade object.</returns>
    TScript GetScript<TScript>(string name) where TScript : RecentScriptFacade;

    /// <summary>
    /// Creates a new script in the database from source code and returns its facade.
    /// </summary>
    /// <typeparam name="TScript">The specific script facade type.</typeparam>
    /// <param name="sourceCode">The raw C# code to create the script from.</param>
    /// <returns>The script facade object.</returns>
    Task<TScript> CreateScript<TScript>(string sourceCode) where TScript : RecentScriptFacade;
}