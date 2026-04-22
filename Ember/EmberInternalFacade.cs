namespace Ember.Simulation;



internal class EmberInternalFacade
{
    private IScriptManagerExtended _scriptManager;
    public EmberInternalFacade(IScriptManagerExtended scriptManager)
    {
        _scriptManager = scriptManager;
    }

    private async Task<object> BaseExecute(Guid id, RecentContext ctx, string methodName)
    {
        CustomerScript script = await _scriptManager.GetScript(id);
        Context context = await CreateByDowngrade(script.ScriptApiVersion, ctx);
        return await _scriptManager.ExecuteScript(id, context, methodName: methodName);
    }
    private object CheckUpgradeActionResult(object result)
    {
        if (result is IUpgradeableReturnValue upgradeableReturnValue)
        {
            return UpgradeManager.UpgradeCustomReturn(result);
        }
        return result;
    }
    internal async Task<object> ExecuteScript(Guid id, RecentContext ctx, string methodName)
    {
        var result = await BaseExecute(id, ctx, methodName);
        return CheckUpgradeActionResult(result);
    }
    internal async Task<object> ExecuteScript<ScriptType>(string name, RecentContext ctx, string methodName)
    where ScriptType : IScriptType
    {
        Guid id = await _scriptManager.GetScriptId<ScriptType>(name);
        return await ExecuteScript(id, ctx, methodName);
    }
    public async Task<object> ExecuteUnfinishedScriptBySourceCode(string sourceCode, RecentContext context, string methodName)
    {
        ValidationRecord vali = _scriptManager.BasicValidationBeforeCompiling(sourceCode);
        Context ctx = await CreateByDowngrade(vali.Version, context);

        var result = await _scriptManager.ExecuteUnfinishedScriptBySourceCode(sourceCode, ctx, methodName: methodName);

        return CheckUpgradeActionResult(result);
    }
    public ScriptFacade<ScriptType> GetScript<ScriptType>(string name) where ScriptType : IScriptType
    {
        ScriptFacade<ScriptType> toReturn = new ScriptFacade<ScriptType>(_scriptManager, name);
        return toReturn;
    }
    public static async Task<Context> CreateByDowngrade(int desiredVersion, RecentContext ctx)
    {
        return await ContextManager.CreateByDowngrade(desiredVersion, ctx);
    }
}
