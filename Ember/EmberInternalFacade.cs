using Ember.Scripting;
using IGeneratorContext_V4;

namespace Ember.Simulation;

internal class EmberInternalFacade
{
    private IScriptManagerDeleteAfter _scriptManager;
    public EmberInternalFacade(IScriptManagerDeleteAfter scriptManager)
    {
        _scriptManager = scriptManager;
    }

    private async Task<object> BaseExecute(Guid id, RecentGeneratorContext ctx, string? methodName = null)
    {
        var cf = new ContextManagement(_scriptManager);
        CustomerScript script = await _scriptManager.GetScript(id);
        GeneratorContextSF context = await cf.CreateByDowngrade(script.MinApiVersion, ctx);
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
            return (bool)result;
        }
        throw new Exception(message: "Type was" + result.GetType().Name);
    }
    internal async Task<object> ExecuteScript(Guid id, RecentGeneratorContext ctx, string? methodName = null)
    {
        var result = await BaseExecute(id, ctx, methodName);
        return CheckUpgradeActionResult(result);
    }
    internal async Task<object> ExecuteScript<ScriptType>(string name, RecentGeneratorContext ctx, string? methodName = null)
    where ScriptType : IScript
    {
        Guid id = await _scriptManager.GetScriptId<ScriptType>(name);
        return await ExecuteScript(id, ctx, methodName);
    }
    public async Task<object> ExecuteUnfinishedScriptBySourceCode(string sourceCode, RecentGeneratorContext context, string? methodName = null)
    {
        var cf = new ContextManagement(_scriptManager);
        ValidationRecord vali = _scriptManager.BasicValidationBeforeCompiling(sourceCode);
        GeneratorContextSF ctx = await cf.CreateByDowngrade(vali.Version, context);

        var result = await _scriptManager.ExecuteUnfinishedScriptBySourceCode(sourceCode, ctx, methodName: methodName);

        return CheckUpgradeActionResult(result);
    }

    public TScript GetScript<TScript>(string name) where TScript : IScript
    {
        if (typeof(TScript) == typeof(IGeneratorConditionScript))
        {
            var toReturn = (IGeneratorConditionScript)new ConditionScriptFacade(_scriptManager, name);
            return (TScript)toReturn;
        }
        else if (typeof(TScript) == typeof(IGeneratorActionScript))
        {
            var toReturn = (IGeneratorActionScript)new ActionScriptFacade(_scriptManager, name);
            return (TScript)toReturn;
        }
        throw new Exception();
    }
}
internal class ConditionScriptFacade : RecentIGeneratorConditionScript
{
    private EmberInternalFacade _emberScriptManager;
    private string _scriptName;

    public ConditionScriptFacade(IScriptManagerDeleteAfter scriptManager, string scriptName)
    {
        _emberScriptManager = new EmberInternalFacade(scriptManager);
        _scriptName = scriptName;
    }
    public async Task<bool> EvaluateAsync(RecentIGeneratorContext context)
    {
        return (bool)await _emberScriptManager.ExecuteScript<IGeneratorConditionScript>(_scriptName, (RecentGeneratorContext)context, null);
    }
}

internal class ActionScriptFacade : RecentIGeneratorActionScript
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
    public async Task<RecentActionResult> ExecuteAsync(RecentIGeneratorContext context)
    {
        return (RecentActionResult)await _emberScriptManager.ExecuteScript<IGeneratorActionScript>(_scriptName, (RecentGeneratorContext)context, null);
    }

    public async Task<RecentActionResult> Execute1(RecentIGeneratorContext context)
    {
        // return (RecentActionResult)await _emberScriptManager.Execute1<IGeneratorActionScript>(_scriptName, (RecentGeneratorContext)context);

        string methodName = nameof(Ember.Scripting.AdditionalMethods.IExecute1.Execute1);
        methodName = MethodNameFactory.GetSafeMethodName(methodName);
        Guid id = await _scriptManager.GetScriptId<IGeneratorActionScript>(_scriptName);
        return (RecentActionResult)await _emberScriptManager.ExecuteScript(id, (RecentGeneratorContext)context, methodName);
    }
    public async Task<RecentActionResult> Execute2(RecentIGeneratorContext context)
    {
        // return (RecentActionResult)await _emberScriptManager.Execute2<IGeneratorActionScript>(_scriptName, (RecentGeneratorContext)context);

        string methodName = nameof(Ember.Scripting.AdditionalMethods.IExecute2.Execute2);
        methodName = MethodNameFactory.GetSafeMethodName(methodName);
        Guid id = await _scriptManager.GetScriptId<IGeneratorActionScript>(_scriptName);
        return (RecentActionResult)await _emberScriptManager.ExecuteScript(id, (RecentGeneratorContext)context, methodName);
    }
}
internal static class MethodNameFactory
{
    public static string GetSafeMethodName(string methodName)
    {
        switch (methodName)
        {
            case "ExecuteAction1":
                return "Execute1";

            case "ExecuteAction2":
                return "Execute2";

            case "ExecuteSomething1":
                return "Execute1";
            default:
                return methodName;
        }
    }
}