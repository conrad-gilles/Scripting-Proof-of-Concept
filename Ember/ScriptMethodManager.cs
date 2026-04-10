using Ember.Scripting;
using Ember.Simulation;
using IGeneratorContext_V4;
using Ember.Scripting.AdditionalMethods;


namespace ScriptMethodManager;

public class MultipleMethodsScriptHelper
{

    IScriptManagerDeleteAfter ScriptManager;
    EmberInternalFacade InternalScriptManager;
    string scriptName;

    internal MultipleMethodsScriptHelper(IScriptManagerDeleteAfter scriptManager, EmberInternalFacade internalScriptManager, string scriptName)
    {
        ScriptManager = scriptManager;
        InternalScriptManager = internalScriptManager;
        this.scriptName = scriptName;
    }

    public async Task<RecentActionResult> ExecuteAsync(RecentContext context)
    {
        return (RecentActionResult)await InternalScriptManager!.ExecuteScript<IActionScript>     //rename to ExecuteScript
       (scriptName, context, methodName: "ExecuteAsync");
    }

    public async Task<RecentActionResult> Execute1(RecentContext context)
    {
        return (RecentActionResult)await InternalScriptManager!.ExecuteScript<IActionScript>
      (scriptName, context, methodName: "Execute1");
    }
    public async Task<RecentActionResult> Execute2(RecentContext context)
    {
        return (RecentActionResult)await InternalScriptManager!.ExecuteScript<IActionScript>
      (scriptName, context, methodName: "Execute2");
    }
}

public class GeneratorScriptFacade : IMultiMethodBase
// public class GeneratorScriptFacade : Ember.Scripting.IGeneratorActionScript, IExecute1, IExecute2
{
    private CustomerScript _script;
    private IScriptManagerDeleteAfter _scriptManager;
    private EmberInternalFacade _emberScriptManager;
    public GeneratorScriptFacade(CustomerScript script, IScriptManagerDeleteAfter scriptManager)
    {
        _script = script;
        _scriptManager = scriptManager;
        _emberScriptManager = new EmberInternalFacade(_scriptManager);
    }
    public Task<ActionResultSF> ExecuteAsync(IGeneratorBaseInterfaceSF context)
    {
        throw new NotImplementedException();
    }
    public Task<ActionResultSF> Execute1(IGeneratorBaseInterfaceSF context)
    {
        throw new NotImplementedException();
    }

    public Task<ActionResultSF> Execute2(IGeneratorBaseInterfaceSF context)
    {
        throw new NotImplementedException();
    }



    // public async Task<ActionResultSF> ExecuteAsync(IGeneratorBaseInterfaceSF context)
    // {
    //     //     return (ActiveActionResult)await _emberScriptManager!.ExecuteScript<IGeneratorActionScript>
    //     //   (_script.ScriptName!, context, methodName: nameof(ExecuteAction1));
    //     return null;
    // }
    // public async Task<ActiveActionResult> ExecuteAction1(ActiveGeneratorContext context)
    // {
    //     return (ActiveActionResult)await _emberScriptManager!.ExecuteScript<IGeneratorActionScript>
    //   (_script.ScriptName!, context, methodName: nameof(ExecuteAction1));
    // }
    // public async Task<ActiveActionResult> ExecuteAction2(ActiveGeneratorContext context)
    // {
    //     return await _emberScriptManager!.ExecuteActionScript<IGeneratorActionScript>
    //   (_script.ScriptName!, context, methodName: nameof(ExecuteAction2));
    // }
}
