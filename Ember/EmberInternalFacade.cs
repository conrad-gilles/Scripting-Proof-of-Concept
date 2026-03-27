using Ember.Scripting;
namespace Ember.Simulation;

internal class EmberInternalFacade
{
    ISccriptManagerDeleteAfter ScriptManager;

    public EmberInternalFacade(ISccriptManagerDeleteAfter scriptManager)
    {
        ScriptManager = scriptManager;
    }

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

    public async Task<ActiveActionResult> ExecuteScript<ScriptType>(string name, ActiveGeneratorContext ctx) where ScriptType : IScript
    {
        // ScriptTypes scriptType;
        // if (typeof(ScriptType) == typeof(IGeneratorActionScript))
        // {
        //     scriptType = ScriptTypes.GeneratorActionScript;
        // }
        // else if (typeof(ScriptType) == typeof(IGeneratorConditionScript))
        // {
        //     scriptType = ScriptTypes.GeneratorConditionScript;
        // }
        // else
        // {
        //     throw new Exception();
        // }

        // // switch (typeof(ScriptType))
        // // {
        // //     case IGeneratorActionScript:
        // //         scriptType = ScriptTypes.GeneratorActionScript;
        // //         break;
        // //     case IGeneratorConditionScript:
        // //         scriptType = ScriptTypes.GeneratorConditionScript;
        // //         break;
        // //     default:
        // //         throw new Exception();
        // // }
        Guid id = await ScriptManager.GetScriptId<ScriptType>(name);
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