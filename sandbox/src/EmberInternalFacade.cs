using Ember.Scripting;

internal class EmberInternalFacade
{
    ISccriptManagerDeleteAfter ScriptManager;

    public EmberInternalFacade(ISccriptManagerDeleteAfter scriptManager)
    {
        ScriptManager = scriptManager;
    }

    public async Task<ActiveActionResult> ExecuteScriptById(Guid id, ActiveDataClass data)
    {
        var cf = new ContextFactory(ScriptManager);
        GeneratorContextSF ctx = await cf.CreateByDowngrade(id, data);
        var result = await ScriptManager.ExecuteScriptById(id, ctx);
        ActiveActionResult ar = (ActiveActionResult)EmberMethods.UpgradeActionResult(result);
        return ar;
    }


}