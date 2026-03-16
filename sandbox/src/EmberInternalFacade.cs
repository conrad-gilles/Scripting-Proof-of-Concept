using Ember.Scripting;

internal class EmberInternalFacade
{
    ISccriptManagerDeleteAfter ScriptManager;

    public EmberInternalFacade(ISccriptManagerDeleteAfter scriptManager)
    {
        ScriptManager = scriptManager;
    }

    // public async Task<ActiveActionResult> ExecuteScriptById(Guid id, ActiveDataClass data)
    public async Task<ActiveActionResult> ExecuteScriptById(Guid id, ActiveGeneratorContext ctx)
    {
        var cf = new ContextFactory(ScriptManager);
        // GeneratorContextSF ctx = await cf.CreateByDowngrade(id, data);
        // ctx = await cf.CreateByDowngrade(id, ctx);
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