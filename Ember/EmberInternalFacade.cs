using Ember.Scripting;
using IGeneratorContext_V4;
using Microsoft.CodeAnalysis.Scripting;

namespace Ember.Simulation;

internal class EmberInternalFacade
{
    private IScriptManagerDeleteAfter _scriptManager;
    public EmberInternalFacade(IScriptManagerDeleteAfter scriptManager)
    {
        _scriptManager = scriptManager;
    }

    private async Task<object> BaseExecute(Guid id, RecentContext ctx, string? methodName = null)
    {
        var cf = new ContextManagement(_scriptManager);
        CustomerScript script = await _scriptManager.GetScript(id);
        methodName = MethodNameFactory.GetOldMethodName(methodName, script.MinApiVersion, script.GetScriptType());
        Context context = await cf.CreateByDowngrade(script.MinApiVersion, ctx);
        return await _scriptManager.ExecuteScriptById(id, context, methodName: methodName);
    }
    private object CheckUpgradeActionResult(object result)
    {
        if (result is ActionResultSF)
        {
            return (RecentActionResult)EmberMethods.UpgradeActionResult(result);
        }
        if (result.GetType() == typeof(bool))
        {
            return result;
        }
        throw new Exception(message: "Type was" + result.GetType().Name);
    }
    internal async Task<object> ExecuteScript(Guid id, RecentContext ctx, string? methodName = null)
    {
        var result = await BaseExecute(id, ctx, methodName);
        return CheckUpgradeActionResult(result);
    }
    internal async Task<object> ExecuteScript<ScriptType>(string name, RecentContext ctx, string? methodName = null)
    where ScriptType : IScript
    {
        Guid id = await _scriptManager.GetScriptId<ScriptType>(name);
        return await ExecuteScript(id, ctx, methodName);
    }
    public async Task<object> ExecuteUnfinishedScriptBySourceCode(string sourceCode, RecentContext context, string? methodName = null)
    {
        var cf = new ContextManagement(_scriptManager);
        ValidationRecord vali = _scriptManager.BasicValidationBeforeCompiling(sourceCode);
        Context ctx = await cf.CreateByDowngrade(vali.Version, context);

        var result = await _scriptManager.ExecuteUnfinishedScriptBySourceCode(sourceCode, ctx, methodName: methodName);

        return CheckUpgradeActionResult(result);
    }

    public TScript GetScript<TScript>(string name) where TScript : IScript
        // public TScript GetScript<TScript>(string name) where TScript : RecentIActionScript
    {
        if (typeof(IConditionScript).IsAssignableFrom(typeof(TScript)))
        {
            var toReturn = (IConditionScript)new ConditionScriptFacade(_scriptManager, name);
            return (TScript)toReturn;
        }
        else if (typeof(IActionScript).IsAssignableFrom(typeof(TScript)))
        {
            var toReturn = (IActionScript)new ActionScriptFacade(_scriptManager, name);
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
        return (bool)await _emberScriptManager.ExecuteScript<IConditionScript>(_scriptName, (RecentContext)context, null);
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
        return (RecentActionResult)await _emberScriptManager.ExecuteScript<IActionScript>(_scriptName, (RecentContext)context, null);
    }

    public async Task<RecentActionResult> Execute1(RecentIContext context)
    {
        string? methodName = nameof(Ember.Scripting.AdditionalMethods.IExecute1.Execute1);
        CustomerScript script = await _scriptManager.GetScriptNT<IActionScript>(_scriptName);
        methodName = MethodNameFactory.GetOldMethodName(methodName, script.MinApiVersion, script.GetScriptType());  //todo fix this 
        return (RecentActionResult)await _emberScriptManager.ExecuteScript<IActionScript>(_scriptName, (RecentContext)context, methodName);
    }
    public async Task<RecentActionResult> Execute2(RecentIContext context)
    {
        string methodName = nameof(Ember.Scripting.AdditionalMethods.IExecute2.Execute2);
        return (RecentActionResult)await _emberScriptManager.ExecuteScript<IActionScript>(_scriptName, (RecentContext)context, methodName);
    }
}
internal static class MethodNameFactory
{
    public static string? GetOldMethodName(string? methodName, int version, Type scriptType)
    {
        if (scriptType == typeof(IActionScript))
        {
            if (methodName == nameof(Ember.Scripting.AdditionalMethods.IExecute1.Execute1))
            {
                switch (version)
                {
                    case 10:
                        return "Execute1";
                    default:
                        return methodName;
                }
            }
            if (methodName == nameof(Ember.Scripting.AdditionalMethods.IExecute2.Execute2))
            {
                switch (version)
                {
                    case 10:
                        return "ExecuteAction2";
                    default:
                        return methodName;
                }
            }
        }
        if (scriptType == typeof(IConditionScript))
        {
            return methodName;
        }
        return methodName;
    }
}