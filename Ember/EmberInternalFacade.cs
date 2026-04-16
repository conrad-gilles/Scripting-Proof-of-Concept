// using Ember.Scripting;
// using IGeneratorContext_V4;
// using Microsoft.CodeAnalysis.Scripting;
// using Ember.Scripting.Compilation;

namespace Ember.Simulation;



internal class EmberInternalFacade
{
    private IScriptManagerDeleteAfter _scriptManager;
    public EmberInternalFacade(IScriptManagerDeleteAfter scriptManager)
    {
        _scriptManager = scriptManager;
    }

    private async Task<object> BaseExecute(Guid id, RecentContext ctx, string methodName)
    {
        var cf = new ContextManagement(_scriptManager);
        CustomerScript script = await _scriptManager.GetScript(id);
        // methodName = MethodNameFactory.GetOldMethodName(methodName, script.MinApiVersion, script.GetScriptType());
        Context context = await cf.CreateByDowngrade(script.MinApiVersion, ctx);
        return await _scriptManager.ExecuteScriptById(id, context, methodName: methodName);
    }
    private object CheckUpgradeActionResult(object result)
    {
        if (result is UpgradeableReturnValue upgradeableReturnValue)
        {
            // return upgradeableReturnValue.Upgrade(result);
            return EmberMethods.UpgradeObject(result);
        }
        return result;
        // if (result is ActionResultSF)
        // {
        //     return (RecentActionResult)EmberMethods.UpgradeActionResult(result);
        // }
        // if (result.GetType() == typeof(bool))
        // {
        //     return result;
        // }
        // if (result.GetType() == typeof(string))
        // {
        //     return result;
        // }
        // throw new Exception(message: "Type was" + result.GetType().Name);
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
        var cf = new ContextManagement(_scriptManager);
        ValidationRecord vali = _scriptManager.BasicValidationBeforeCompiling(sourceCode);
        Context ctx = await cf.CreateByDowngrade(vali.Version, context);

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
}
internal class ConditionScriptFacade : RecentIConditionScript
{
    private EmberInternalFacade _emberScriptManager;
    private string _scriptName;

    public ConditionScriptFacade(IScriptManagerDeleteAfter scriptManager, string scriptName)
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
    private IScriptManagerDeleteAfter _scriptManager;
    private string _scriptName;

    public ActionScriptFacade(IScriptManagerDeleteAfter scriptManager, string scriptName)
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
        CustomerScript script = await _scriptManager.GetScriptNT<IActionScript>(_scriptName);   // this is being called twice, also in ExecuteScript i can move it down but then i also need to move the old MethodName check down into ExecuteScript 
        if (script.MinApiVersion == 10)
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