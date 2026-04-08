using Ember.Scripting;
using IGeneratorContext_V4;

namespace Ember.Simulation;

internal class EmberInternalFacade
{
    private IScriptManagerDeleteAfter _scriptManager;

    // private string? _scriptName = null;
    // private Type? _scriptType = null;

    public EmberInternalFacade(IScriptManagerDeleteAfter scriptManager)
    {
        _scriptManager = scriptManager;
    }

    // public EmberInternalFacade(IScriptManagerDeleteAfter scriptManager, string scriptName, Type scriptType)
    // {
    //     _scriptManager = scriptManager;
    //     _scriptName = scriptName;
    //     _scriptType = scriptType;
    // }

    private async Task<object> BaseExecute(Guid id, ActiveGeneratorContext ctx, string? methodName = null)
    {
        var cf = new ContextManagement(_scriptManager);
        CustomerScript script = await _scriptManager.GetScript(id);
        GeneratorContextSF context = await cf.CreateByDowngrade(script.MinApiVersion, ctx);
        return await _scriptManager.ExecuteScriptById(id, context, methodName: methodName);
    }
    private object CheckUpgradeActionResult(object result)
    {
        if (result.GetType() != typeof(bool))
        {
            return (ActiveActionResult)EmberMethods.UpgradeActionResult(result);    //maybe check type with try?
        }
        return result;
    }
    public async Task<object> ExecuteScript(Guid id, ActiveGeneratorContext ctx, string? methodName = null)
    {
        var result = await BaseExecute(id, ctx, methodName);
        return CheckUpgradeActionResult(result);
    }
    public async Task<object> ExecuteScript<ScriptType>(string name, ActiveGeneratorContext ctx, string? methodName = null)
    where ScriptType : IScript
    {
        Guid id = await _scriptManager.GetScriptId<ScriptType>(name);
        return await ExecuteScript(id, ctx, methodName);
    }
    public async Task<object> Execute1<ScriptType>(string name, ActiveGeneratorContext ctx)
    where ScriptType : IScript
    {
        string methodName = nameof(Ember.Scripting.AdditionalMethods.IExecute1.Execute1);
        methodName = MethodNameFactory.GetSafeMethodName(methodName);
        Guid id = await _scriptManager.GetScriptId<ScriptType>(name);
        return await ExecuteScript(id, ctx, methodName);
    }
    public async Task<object> Execute2<ScriptType>(string name, ActiveGeneratorContext ctx)
    where ScriptType : IScript
    {
        string methodName = nameof(Ember.Scripting.AdditionalMethods.IExecute2.Execute2);
        methodName = MethodNameFactory.GetSafeMethodName(methodName);
        Guid id = await _scriptManager.GetScriptId<ScriptType>(name);
        return await ExecuteScript(id, ctx, methodName);
    }
    public async Task<object> ExecuteUnfinishedScriptBySourceCode(string sourceCode, ActiveGeneratorContext context, string? methodName = null)
    {
        var cf = new ContextManagement(_scriptManager);
        ValidationRecord vali = _scriptManager.BasicValidationBeforeCompiling(sourceCode);
        GeneratorContextSF ctx = await cf.CreateByDowngrade(vali.Version, context);

        var result = await _scriptManager.ExecuteUnfinishedScriptBySourceCode(sourceCode, ctx, methodName: methodName);

        return CheckUpgradeActionResult(result);
    }

    public ScriptFacade<ScriptType> GetActionScript<ScriptType>(string name) where ScriptType : IScript//for condition script this doesnt make sense?
    {
        return new ScriptFacade<ScriptType>(_scriptManager, name);
    }
}

internal class ScriptFacade<ScriptType> : ActiveIGeneratorActionScript,
GeneratorScriptsGenericSimple.IGeneratorConditionScript<ActiveIGeneratorContext> where ScriptType : IScript
{
    private EmberInternalFacade _emberScriptManager;
    private string _scriptName;

    public ScriptFacade(IScriptManagerDeleteAfter scriptManager, string scriptName)
    {
        _emberScriptManager = new EmberInternalFacade(scriptManager);
        _scriptName = scriptName;
    }
    public async Task<bool> EvaluateAsync(ActiveIGeneratorContext context)
    {
        return (bool)await _emberScriptManager.ExecuteScript<ScriptType>(_scriptName, (ActiveGeneratorContext)context, null);
    }
    public async Task<ActiveActionResult> ExecuteAsync(ActiveIGeneratorContext context)
    {
        // GeneratorContextNoInherVaccineV5.GeneratorContext
        return (ActiveActionResult)await _emberScriptManager.ExecuteScript<ScriptType>(_scriptName, (ActiveGeneratorContext)context, null);
    }

    public async Task<ActionResultV3.ActionResult> Execute1(ActiveIGeneratorContext context)
    {
        return (ActiveActionResult)await _emberScriptManager.Execute1<ScriptType>(_scriptName, (ActiveGeneratorContext)context);
    }
    public async Task<ActionResultV3.ActionResult> Execute2(ActiveIGeneratorContext context)
    {

        return (ActiveActionResult)await _emberScriptManager.Execute2<ScriptType>(_scriptName, (ActiveGeneratorContext)context);
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