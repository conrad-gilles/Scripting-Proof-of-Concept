// using Ember.Scripting;
// using IGeneratorContext_V4;
// using Microsoft.CodeAnalysis.Scripting;
// using Ember.Scripting.Compilation;

namespace Ember.Simulation;



internal class EmberInternalFacade
{
    private IScriptManagerExtended _scriptManager;
    public EmberInternalFacade(IScriptManagerExtended scriptManager)
    {
        _scriptManager = scriptManager;
    }

    private async Task<object> BaseExecute(Guid id, RecentContext ctx, string methodName)
    {
        CustomerScript script = await _scriptManager.GetScript(id);
        // methodName = MethodNameFactory.GetOldMethodName(methodName, script.MinApiVersion, script.GetScriptType());
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

    public TScript GetScript<TScript>(string name) where TScript : IScriptMethod
    {
        if (typeof(IScriptMethodsCondition).IsAssignableFrom(typeof(TScript)))
        {
            var toReturn = (IScriptMethodsCondition)new ConditionScriptFacade(_scriptManager, name);
            return (TScript)toReturn;
        }
        else if (typeof(IScriptMethodsAction).IsAssignableFrom(typeof(TScript)))
        {
            var toReturn = (IScriptMethodsAction)new ActionScriptFacade(_scriptManager, name);
            return (TScript)toReturn;
        }
        throw new Exception();
    }
    public static async Task<Context> CreateByDowngrade(int desiredVersion, RecentContext ctx)
    {
        return await ContextManager.CreateByDowngrade(desiredVersion, ctx);
    }
}
internal class ConditionScriptFacade : RecentIConditionScript
{
    private EmberInternalFacade _emberScriptManager;
    private string _scriptName;

    public ConditionScriptFacade(IScriptManagerExtended scriptManager, string scriptName)
    {
        _emberScriptManager = new EmberInternalFacade(scriptManager);
        _scriptName = scriptName;
    }
    public async Task<bool> EvaluateAsync(RecentIContext context)
    {
        string methodName = nameof(Ember.Sandbox.ScriptMethods.IEvaluateAsync.EvaluateAsync);
        return (bool)await _emberScriptManager.ExecuteScript<IConditionScript>(_scriptName, (RecentContext)context, methodName);
    }
}

internal class ActionScriptFacade : RecentIActionScript
{
    private EmberInternalFacade _emberScriptManager;
    private IScriptManagerExtended _scriptManager;
    private string _scriptName;

    public ActionScriptFacade(IScriptManagerExtended scriptManager, string scriptName)
    {
        _scriptManager = scriptManager;
        _emberScriptManager = new EmberInternalFacade(scriptManager);
        _scriptName = scriptName;
    }
    public async Task<RecentActionResult> ExecuteAsync(RecentIContext context)
    {
        string methodName = nameof(IExecuteAsync.ExecuteAsync);
        return (RecentActionResult)await _emberScriptManager.ExecuteScript<IActionScript>(_scriptName, (RecentContext)context, methodName);
    }

    public async Task<RecentActionResult> Execute1(RecentIContext context)
    {
        string methodName = nameof(IExecute1.Execute1);
        CustomerScript script = await _scriptManager.GetScript<IActionScript>(_scriptName);   // this is being called twice, also in ExecuteScript i can move it down but then i also need to move the old MethodName check down into ExecuteScript 
        if (script.ScriptApiVersion == 10)
        {
            methodName = "ExecuteOldName1";
        }
        return (RecentActionResult)await _emberScriptManager.ExecuteScript<IActionScript>(_scriptName, (RecentContext)context, methodName);
    }
    public async Task<RecentActionResult> Execute2(RecentIContext context)
    {
        string methodName = nameof(IExecute2.Execute2);
        return (RecentActionResult)await _emberScriptManager.ExecuteScript<IActionScript>(_scriptName, (RecentContext)context, methodName);
    }
}

// internal static class MethodNameFactory //todo delete
// {
//     public static string GetOldMethodName(string methodName, int version, Type scriptType)
//     {
//         if (scriptType == typeof(IActionScript))
//         {
//             if (methodName == nameof(Ember.Scripting.ScriptMethods.IExecute1.Execute1))
//             {
//                 switch (version)
//                 {
//                     case 10:
//                         return "Execute1";
//                     default:
//                         return methodName;
//                 }
//             }
//             if (methodName == nameof(Ember.Scripting.ScriptMethods.IExecute2.Execute2))
//             {
//                 switch (version)
//                 {
//                     case 10:
//                         return "ExecuteAction2";
//                     default:
//                         return methodName;
//                 }
//             }
//         }
//         if (scriptType == typeof(IConditionScript))
//         {
//             return methodName;
//         }
//         return methodName;
//     }
// }