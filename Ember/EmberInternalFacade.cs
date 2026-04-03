using Ember.Scripting;
namespace Ember.Simulation;

internal class EmberInternalFacade
{
    private IScriptManagerDeleteAfter _scriptManager;

    public EmberInternalFacade(IScriptManagerDeleteAfter scriptManager)
    {
        _scriptManager = scriptManager;
    }

    private async Task<object> BaseExecute(Guid id, ActiveGeneratorContext ctx, string? methodName = null)
    {
        var cf = new ContextManagement(_scriptManager);
        CustomerScript script = await _scriptManager.GetScript(id);
        GeneratorContextSF context = await cf.CreateByDowngrade(script.SourceCode!, ctx);
        var result = await _scriptManager.ExecuteScriptById(id, context, methodName: methodName);
        return result;
    }
    private object CheckUpgradeActionResult(object result)
    {
        if (result.GetType() != typeof(bool))
        {
            ActiveActionResult ar = (ActiveActionResult)EmberMethods.UpgradeActionResult(result);
            return ar;
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
    public async Task<ActiveActionResult> ExecuteActionScript<ActionType>
    (string name, ActiveGeneratorContext ctx, string? methodName = null) where ActionType : IGeneratorActionScript
    {
        Guid id = await _scriptManager.GetScriptId<ActionType>(name);
        return (ActiveActionResult)await ExecuteScript(id, ctx, methodName);
    }
    public async Task<bool> ExecuteConditionScript<ConditionType>(string name, ActiveGeneratorContext ctx, string? methodName = null)
    where ConditionType : IGeneratorConditionScript
    {
        Guid id = await _scriptManager.GetScriptId<ConditionType>(name);
        return (bool)await ExecuteScript(id, ctx, methodName);
    }

    public async Task<object> ExecuteUnfinishedScriptBySourceCode(string sourceCode, ActiveGeneratorContext context, string? methodName = null)
    {
        var cf = new ContextManagement(_scriptManager);
        GeneratorContextSF ctx = await cf.CreateByDowngrade(sourceCode, context);

        var result = await _scriptManager.ExecuteUnfinishedScriptBySourceCode(sourceCode, ctx, methodName: methodName);

        return CheckUpgradeActionResult(result);
    }
}