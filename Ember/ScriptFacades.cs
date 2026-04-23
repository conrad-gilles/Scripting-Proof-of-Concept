namespace Ember.Simulation;

internal abstract class RecentScriptFacade
{

}
internal class ConditionScript : RecentScriptFacade, RecentIConditionScript
{
    private EmberInternalFacade _emberScriptManager;
    private string _scriptName;

    public ConditionScript(IScriptManagerExtended scriptManager, string scriptName)
    {
        _emberScriptManager = new EmberInternalFacade(scriptManager);
        _scriptName = scriptName;
    }
    public async Task<bool> EvaluateAsync(RecentIContext context)
    {
        string methodName = nameof(RecentIConditionScript.EvaluateAsync);
        return (bool)await _emberScriptManager.ExecuteScript<IConditionScript>(_scriptName, (RecentContext)context, methodName);

    }
}

internal class ActionScript : RecentScriptFacade, RecentIActionScript
{
    private EmberInternalFacade _emberScriptManager;
    private IScriptManagerExtended _scriptManager;
    private string _scriptName;

    public ActionScript(IScriptManagerExtended scriptManager, string scriptName)
    {
        _scriptManager = scriptManager;
        _emberScriptManager = new EmberInternalFacade(scriptManager);
        _scriptName = scriptName;
    }
    public async Task<RecentActionResult> ExecuteAsync(RecentIContext context)
    {
        string methodName = nameof(GeneratorScriptsV4.IActionScript.ExecuteAsync);
        return (RecentActionResult)await _emberScriptManager.ExecuteScript<IActionScript>(_scriptName, (RecentContext)context, methodName);
    }

    public async Task<RecentActionResult> Execute1(RecentIContext context)
    {
        string methodName = nameof(GeneratorScriptsV4.IActionScript.Execute1);
        CustomerScript script = await _scriptManager.GetScript<IActionScript>(_scriptName);   // this is being called twice, also in ExecuteScript i can move it down but then i also need to move the old MethodName check down into ExecuteScript 
        if (script.ScriptApiVersion == 3)
        {
            methodName = "Execute1OldName";
        }
        return (RecentActionResult)await _emberScriptManager.ExecuteScript<IActionScript>(_scriptName, (RecentContext)context, methodName);
    }
    public async Task<RecentActionResult> Execute2(RecentIContext context)
    {
        string methodName = nameof(GeneratorScriptsV4.IActionScript.Execute2);
        return (RecentActionResult)await _emberScriptManager.ExecuteScript<IActionScript>(_scriptName, (RecentContext)context, methodName);
    }
    public async Task<string> Execute3(RecentIContext context)
    {
        string methodName = nameof(GeneratorScriptsV4.IActionScript.Execute3);
        return (string)await _emberScriptManager.ExecuteScript<IActionScript>(_scriptName, (RecentContext)context, methodName);
    }
}