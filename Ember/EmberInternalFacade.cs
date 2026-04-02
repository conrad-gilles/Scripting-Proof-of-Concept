using Ember.Scripting;
namespace Ember.Simulation;

internal class EmberInternalFacade
{
    private ISccriptManagerDeleteAfter _scriptManager;

    public EmberInternalFacade(ISccriptManagerDeleteAfter scriptManager)
    {
        _scriptManager = scriptManager;
    }

    public async Task<ActiveActionResult> ExecuteScript(Guid id, ActiveGeneratorContext ctx, string? methodName = null)
    {
        var cf = new ContextManagement(_scriptManager);
        // GeneratorContextSF ctx = await cf.CreateByDowngrade(id, data);
        // ctx = await cf.CreateByDowngrade(id, ctx);
        CustomerScript script = await _scriptManager.GetScript(id);
        GeneratorContextSF context = await cf.CreateByDowngrade(script.SourceCode!, ctx);
        var result = await _scriptManager.ExecuteScriptById(id, context, methodName: methodName);
        ActiveActionResult ar = (ActiveActionResult)EmberMethods.UpgradeActionResult(result);
        return ar;
    }

    public async Task<ActiveActionResult> ExecuteScript<ScriptType>(string name, ActiveGeneratorContext ctx, string? methodName = null) where ScriptType : IScript
    {
        Guid id = await _scriptManager.GetScriptId<ScriptType>(name);
        CustomerScript script = await _scriptManager.GetScript(id);
        var cf = new ContextManagement(_scriptManager);
        GeneratorContextSF context = await cf.CreateByDowngrade(script.SourceCode!, ctx);
        var result = await _scriptManager.ExecuteScriptById(id, context, methodName: methodName);
        ActiveActionResult ar = (ActiveActionResult)EmberMethods.UpgradeActionResult(result);
        return ar;
    }
    public async Task<ActiveActionResult> ExecuteUnfinishedScriptBySourceCode(string sourceCode, ActiveGeneratorContext context, string? methodName = null)
    {
        var cf = new ContextManagement(_scriptManager);
        GeneratorContextSF ctx = await cf.CreateByDowngrade(sourceCode, context);

        var result = await _scriptManager.ExecuteUnfinishedScriptBySourceCode(sourceCode, ctx);

        ActiveActionResult ar = (ActiveActionResult)EmberMethods.UpgradeActionResult(result);
        return ar;
    }
    public ActiveGeneratorContext CreateContext(ILabOrderInterfaceV4NoInheritence labOrder, IVaccineInterface vaccine)
    {
        ActiveGeneratorContext ctx = new ActiveGeneratorContext(labOrder: labOrder, vaccine: vaccine);
        return ctx;
    }
}