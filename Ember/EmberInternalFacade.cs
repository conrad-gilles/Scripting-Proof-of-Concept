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
        if (result.GetType() != typeof(bool))
        {
            return (RecentActionResult)EmberMethods.UpgradeActionResult(result);
        }
        return result;
    }
    public async Task<object> ExecuteScript(Guid id, RecentGeneratorContext ctx, string? methodName = null)
    {
        var result = await BaseExecute(id, ctx, methodName);
        return CheckUpgradeActionResult(result);
    }
    public async Task<object> ExecuteScript<ScriptType>(string name, RecentGeneratorContext ctx, string? methodName = null)
    where ScriptType : IScript
    {
        Guid id = await _scriptManager.GetScriptId<ScriptType>(name);
        return await ExecuteScript(id, ctx, methodName);
    }
    public async Task<object> Execute1<ScriptType>(string name, RecentGeneratorContext ctx)
    where ScriptType : IScript
    {
        string methodName = nameof(Ember.Scripting.AdditionalMethods.IExecute1.Execute1);
        methodName = MethodNameFactory.GetSafeMethodName(methodName);
        Guid id = await _scriptManager.GetScriptId<ScriptType>(name);
        return await ExecuteScript(id, ctx, methodName);
    }
    public async Task<object> Execute2<ScriptType>(string name, RecentGeneratorContext ctx)
    where ScriptType : IScript
    {
        string methodName = nameof(Ember.Scripting.AdditionalMethods.IExecute2.Execute2);
        methodName = MethodNameFactory.GetSafeMethodName(methodName);
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

    public ScriptFacade<ScriptType> GetScript<ScriptType>(string name) where ScriptType : IScript
    {
        return new ScriptFacade<ScriptType>(_scriptManager, name);
    }
}

internal class ScriptFacade<ScriptType> : RecentIGeneratorActionScript,
GeneratorScriptsGenericSimple.IGeneratorConditionScript<RecentIGeneratorContext> where ScriptType : IScript
{
    private EmberInternalFacade _emberScriptManager;
    private string _scriptName;

    public ScriptFacade(IScriptManagerDeleteAfter scriptManager, string scriptName)
    {
        _emberScriptManager = new EmberInternalFacade(scriptManager);
        _scriptName = scriptName;
    }
    public async Task<bool> EvaluateAsync(RecentIGeneratorContext context)
    {
        return (bool)await _emberScriptManager.ExecuteScript<ScriptType>(_scriptName, (RecentGeneratorContext)context, null);
    }
    public async Task<RecentActionResult> ExecuteAsync(RecentIGeneratorContext context)
    {
        return (RecentActionResult)await _emberScriptManager.ExecuteScript<ScriptType>(_scriptName, (RecentGeneratorContext)context, null);
    }

    public async Task<RecentActionResult> Execute1(RecentIGeneratorContext context)
    {
        return (RecentActionResult)await _emberScriptManager.Execute1<ScriptType>(_scriptName, (RecentGeneratorContext)context);
    }
    public async Task<RecentActionResult> Execute2(RecentIGeneratorContext context)
    {

        return (RecentActionResult)await _emberScriptManager.Execute2<ScriptType>(_scriptName, (RecentGeneratorContext)context);
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