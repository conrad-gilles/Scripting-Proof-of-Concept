using Ember.Scripting;
namespace Ember.Simulation;

internal class EmberInternalFacade
{
    ISccriptManagerDeleteAfter ScriptManager;

    public EmberInternalFacade(ISccriptManagerDeleteAfter scriptManager)
    {
        ScriptManager = scriptManager;
    }

    // public async Task<ActiveActionResult> ExecuteScriptById(Guid id, ActiveDataClass data)
    public async Task<ActiveActionResult> ExecuteScript(Guid id, ActiveGeneratorContext ctx)
    {
        var cf = new ContextManagement(ScriptManager);
        // GeneratorContextSF ctx = await cf.CreateByDowngrade(id, data);
        // ctx = await cf.CreateByDowngrade(id, ctx);
        GeneratorContextSF context = await cf.CreateByDowngrade(id, ctx);
        var result = await ScriptManager.ExecuteScriptById(id, context);
        ActiveActionResult ar = (ActiveActionResult)EmberMethods.UpgradeActionResult(result);
        return ar;
    }

    public async Task<ActiveActionResult> ExecuteScript(string name, ScriptTypes scriptType, ActiveGeneratorContext ctx) //maybe make generic
    {
        Guid id = await ScriptManager.GetScriptId(name, scriptType);
        var cf = new ContextManagement(ScriptManager);
        GeneratorContextSF context = await cf.CreateByDowngrade(id, ctx);
        var result = await ScriptManager.ExecuteScriptById(id, context);
        ActiveActionResult ar = (ActiveActionResult)EmberMethods.UpgradeActionResult(result);
        return ar;
    }

    public ActiveGeneratorContext CreateContext(ILabOrderInterfaceV4NoInheritence labOrder, IVaccineInterface vaccine)
    {
        ActiveGeneratorContext ctx = new ActiveGeneratorContext(labOrder: labOrder, vaccine: vaccine);
        return ctx;
    }
}