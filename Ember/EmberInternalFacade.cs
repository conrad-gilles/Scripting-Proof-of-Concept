using Ember.Scripting;
namespace Ember.Simulation;

internal class EmberInternalFacade
{
    private ISccriptManagerDeleteAfter _scriptManager;

    public EmberInternalFacade(ISccriptManagerDeleteAfter scriptManager)
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
    public async Task<object> ExecuteScript(Guid id, ActiveGeneratorContext ctx, string? methodName = null)
    {
        var result = await BaseExecute(id, ctx, methodName);
        return CheckUpgradeActionResult(result);
    }
    public async Task<ActiveActionResult> ExecuteActionScript(Guid id, ActiveGeneratorContext ctx, string? methodName = null)
    {
        var result = await BaseExecute(id, ctx, methodName);
        ActiveActionResult ar = (ActiveActionResult)EmberMethods.UpgradeActionResult(result);
        return ar;
    }

    public async Task<ActiveActionResult> ExecuteActionScript<ScriptType>(string name, ActiveGeneratorContext ctx, string? methodName = null) where ScriptType : IScript
    {
        Guid id = await _scriptManager.GetScriptId<ScriptType>(name);
        return await ExecuteActionScript(id, ctx, methodName);
    }
    public async Task<object> ExecuteUnfinishedScriptBySourceCode(string sourceCode, ActiveGeneratorContext context, string? methodName = null)
    {
        var cf = new ContextManagement(_scriptManager);
        GeneratorContextSF ctx = await cf.CreateByDowngrade(sourceCode, context);

        var result = await _scriptManager.ExecuteUnfinishedScriptBySourceCode(sourceCode, ctx, methodName: methodName);

        return CheckUpgradeActionResult(result);
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
    public ActiveGeneratorContext CreateContext(ILabOrderInterfaceV4NoInheritence labOrder, IVaccineInterface vaccine)
    {
        ActiveGeneratorContext ctx = new ActiveGeneratorContext(labOrder: labOrder, vaccine: vaccine);
        return ctx;
    }
}