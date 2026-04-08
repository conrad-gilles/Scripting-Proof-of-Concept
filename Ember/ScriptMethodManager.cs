using Ember.Scripting;
using Ember.Simulation;


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

    public async Task<ActiveActionResult> ExecuteAsync(ActiveGeneratorContext context)
    {
        return await InternalScriptManager!.ExecuteActionScript<IGeneratorActionScript>     //rename to ExecuteScript
       (scriptName, context, methodName: "ExecuteAsync");
    }

    public async Task<ActiveActionResult> ExecuteAction1(ActiveGeneratorContext context)
    {
        return await InternalScriptManager!.ExecuteActionScript<IGeneratorActionScript>
      (scriptName, context, methodName: "ExecuteAction1");
    }
    public async Task<ActiveActionResult> ExecuteAction2(ActiveGeneratorContext context)
    {
        return await InternalScriptManager!.ExecuteActionScript<IGeneratorActionScript>
      (scriptName, context, methodName: "ExecuteAction2");
    }
}